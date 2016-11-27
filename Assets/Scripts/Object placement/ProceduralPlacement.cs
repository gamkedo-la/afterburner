using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MissionGoals))]
public class ProceduralPlacement : MonoBehaviour
{
    public static float TimePlacementFinished;

    [Header("Mission details")]
    [SerializeField] bool m_trainingMission = false;
    public string missionName;
    [TextArea(1, 2)]
    public string missionGoal;
    [TextArea(3, 10)]
    public string missionStory;

    private MissionGoals m_missionGoals;

    [Header("Options")]
    [SerializeField] bool m_showDebugSpheres = false;
    [SerializeField] bool m_useGlobalSeed = true;
    [SerializeField] int m_seed = 1;
    [SerializeField] int m_maxPlacementAttempts = 100;

    [Header("Main target")]
    [SerializeField] PlaceableObject m_mainTargetPrefab;
    [SerializeField] float m_minDistanceFromPlayer = 800f;
    [SerializeField] float m_maxDistanceFromPlayer = 1200f;
    [SerializeField] float m_minAngleFromNorth = -45f;
    [SerializeField] float m_maxAngleFromNorth = 45f;
    [SerializeField] Vector2 m_groundObjectHeightMinMax = new Vector2(0f, 1000f);
    [SerializeField] Vector2 m_airObjectAltitudeMinMax = new Vector2(100f, 500f);

    [Header("Enemy aircraft")]
    [SerializeField] PlacementOptionsAir[] m_aircraftPlacementOptions;

    [Header("Enemy ground defences")]
    [SerializeField] PlacementOptionsGround[] m_groundDefencesOptions;

    [Header("Enemy water defences")]
    [SerializeField] PlacementOptionsWater[] m_waterDefencesOptions;

    private MapGenerator m_mapGenerator;
    private Vector3 m_playerPosition;
    private Vector3 m_groundZeroPosition;
    private Transform m_mainTargetTransform;

    private List<Vector3> m_groundObjectPositions = new List<Vector3>();
    private List<Vector3> m_waterObjectPositions = new List<Vector3>();
    private List<Vector3> m_airObjectPositions = new List<Vector3>();


    void Awake()
    {
        m_seed = m_useGlobalSeed ? SeedManager.MissionSeed : m_seed;
        //print("Mission seed: " + m_seed);

        CallsignManager.ResetUsedIndices();
        var mapGeneratorObject = GameObject.FindGameObjectWithTag(Tags.MapGenerator);

        if (mapGeneratorObject != null)
            m_mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();

        if (m_mapGenerator == null)
        {
            print(string.Format("No object with the tag '{0}' was found, so object placement couldn't be performed",
                Tags.MapGenerator));
            return;
        }

        m_mapGenerator.Initialise();

        var playerObject = GameObject.FindGameObjectWithTag(Tags.Player);

        if (playerObject != null)
            m_playerPosition = playerObject.transform.position;

        m_missionGoals = GetComponent<MissionGoals>();
        //m_missionGoals.Initialise();

        PlaceMainTarget();
        PlaceEnemyAircraft();
        PlaceEnemyGroundDefences();
        PlaceEnemyWaterDefences();

        TimePlacementFinished = Time.time;

		//print(string.Format("Placement finshed at: {0}", TimePlacementFinished));
		Debug.Log(m_mapGenerator.uniformScale + ", " + MapGenerator.MapChunkSize);

	}


	private void PlaceMainTarget()
    {
        Random.InitState(m_seed);
    
        if (m_mainTargetPrefab == null)
        {
            //print("No main target prefab defined");
            SetBackupGroundZeroPosition();
            return;
        }

        var mainTargetObject = (GameObject) Instantiate(m_mainTargetPrefab.gameObject, Vector3.zero, Quaternion.identity);
        var mainTarget = mainTargetObject.GetComponent<PlaceableObject>();
        mainTarget.gameObject.transform.parent = transform;

        bool success = false;
        int attempts = 1;

        var groundObject = mainTarget as PlaceableObjectGround;
        var waterObject = mainTarget as PlaceableObjectWater;
        var airObject = mainTarget as PlaceableObjectAir;

        while (!success && attempts <= m_maxPlacementAttempts)
        {
            //print(string.Format("Attempt {0}", attempts));
            if (groundObject != null)
                success = TestGroundPosition(groundObject, null, true);
            else if (waterObject != null)
                success = TestWaterPosition(waterObject, null, true);
            else if (airObject != null)
                success = TestAirPosition(airObject, null, true);
            else
                attempts = m_maxPlacementAttempts;

            attempts++;
        }

        if (attempts > m_maxPlacementAttempts)
        {
            //print("Failed to place main target");
            Destroy(mainTarget.gameObject);
        }
        //else
        //    print(string.Format("Main target took {0} attempts to place", --attempts));

        if (mainTarget != null)
        {
            m_groundZeroPosition = mainTarget.transform.position;
            m_mainTargetTransform = mainTarget.transform;
            m_missionGoals.AddMainTarget(mainTarget.transform);
        }
        else
            SetBackupGroundZeroPosition();
    }


    private void SetBackupGroundZeroPosition()
    {
        float distance = Random.Range(m_minDistanceFromPlayer, m_maxDistanceFromPlayer);
        m_groundZeroPosition = m_playerPosition + Vector3.forward * distance;
    }


    private void PlaceEnemyAircraft()
    {
        if (m_aircraftPlacementOptions.Length == 0)
        {
            //print("No enemy aircraft waves are defined");
            return;
        }

        Random.InitState(m_seed + 1);

        for (int k = 0; k < m_aircraftPlacementOptions.Length; k++)
        {
            var options = m_aircraftPlacementOptions[k];
            int number = options.number;
            var aircraftTypePrefabs = options.aircraftTypePrefabs;

            if (number == 0)
            {
                //print(string.Format("No enemy aircraft prefabs defined in wave {0}", k + 1));
                continue;
            }        

            for (int i = 0; i < number; i++)
            {
                int index = Random.Range(0, aircraftTypePrefabs.Length);
                var airGameObject = (GameObject) Instantiate(aircraftTypePrefabs[index].gameObject, Vector3.zero, Quaternion.identity);
                var airObject = airGameObject.GetComponent<PlaceableObjectAir>();
                airGameObject.gameObject.transform.parent = transform;

                bool success = false;
                int attempts = 1;

                while (!success && attempts <= m_maxPlacementAttempts)
                {
                    success = TestAirPosition(airObject, options);
                    attempts++;
                }

                if (attempts > m_maxPlacementAttempts)
                {
                    //print(string.Format("Failed to place enemy aircraft number {0} in wave {1}", i + 1, k + 1));
                    Destroy(airGameObject);
                }
                else
                {
                    m_missionGoals.AddAirObject(airGameObject.transform);

                    if (m_trainingMission)
                    {
                        var enemyAiFlightInput = airGameObject.GetComponent<EnemyAircraftAiInput>();
                        var enemyAiShootingInput = airGameObject.GetComponent<EnemyShootingAiInput>();

                        if (enemyAiFlightInput != null)
                            enemyAiFlightInput.enabled = false;

                        if (enemyAiShootingInput != null)
                            enemyAiShootingInput.enabled = false;
                    }
                }
            }
        }
    }


    private void PlaceEnemyGroundDefences()
    {
        if (m_groundDefencesOptions.Length == 0)
        {
            //print("No enemy ground defence waves are defined");
            return;
        }

        Random.InitState(m_seed + 2);

        for (int k = 0; k < m_groundDefencesOptions.Length; k++)
        {
            var options = m_groundDefencesOptions[k];
            int number = options.number;
            var groundDefenceTypePrefabs = options.groundDefenceTypePrefabs;

            if (number == 0)
            {
                //print(string.Format("No enemy ground defence prefabs defined in wave {0}", k + 1));
                continue;
            }

            for (int i = 0; i < number; i++)
            {
                int index = Random.Range(0, groundDefenceTypePrefabs.Length);
                var groundGameObject = (GameObject) Instantiate(groundDefenceTypePrefabs[index].gameObject, Vector3.zero, Quaternion.identity);
                var groundObject = groundGameObject.GetComponent<PlaceableObjectGround>();
                groundGameObject.gameObject.transform.parent = transform;

                bool success = false;
                int attempts = 1;

                while (!success && attempts <= m_maxPlacementAttempts)
                {
                    success = TestGroundPosition(groundObject, options);
                    attempts++;
                }

                if (attempts > m_maxPlacementAttempts)
                {
                    //print(string.Format("Failed to place enemy ground defence number {0} ({1}) in wave {2}", i + 1, groundGameObject.name, k + 1));
                    Destroy(groundGameObject);
                }
                else
                    m_missionGoals.AddGroundObject(groundGameObject.transform);
            }
        }
    }


    private void PlaceEnemyWaterDefences()
    {
        if (m_waterDefencesOptions.Length == 0)
        {
            //print("No enemy water defence waves are defined");
            return;
        }

        Random.InitState(m_seed + 3);

        for (int k = 0; k < m_waterDefencesOptions.Length; k++)
        {
            var options = m_waterDefencesOptions[k];
            int number = options.number;
            var waterDefenceTypePrefabs = options.waterDefenceTypePrefabs;

            if (number == 0)
            {
                //print(string.Format("No enemy water defence prefabs defined in wave {0}", k + 1));
                continue;
            }

            for (int i = 0; i < number; i++)
            {
                int index = Random.Range(0, waterDefenceTypePrefabs.Length);
                var waterGameObject = (GameObject) Instantiate(waterDefenceTypePrefabs[index].gameObject, Vector3.zero, Quaternion.identity);
                var waterObject = waterGameObject.GetComponent<PlaceableObjectWater>();
                waterGameObject.gameObject.transform.parent = transform;

                bool success = false;
                int attempts = 1;

                while (!success && attempts <= m_maxPlacementAttempts)
                {
                    success = TestWaterPosition(waterObject, options);
                    attempts++;
                }

                if (attempts > m_maxPlacementAttempts)
                {
                    //print(string.Format("Failed to place enemy water defence number {0} in wave {1}", i + 1, k + 1));
                    Destroy(waterGameObject);
                }
                else
                    m_missionGoals.AddWaterObject(waterGameObject.transform);
            }
        }
    }


    private bool GetTrialPosition(PlacementOptions options, 
        bool mainTarget, List<Vector3> objectsToAvoid, out Vector3 position)
    {
        var referencePosition = m_groundZeroPosition;
        float minDist = options != null ? options.minDistFromMainTarget : 2000;
        float maxDist = options != null ? options.maxDistFromMainTarget : 3000;
        float minSeparation = options != null ? options.minSeparation : 200;
        float minAngle = options != null ? options.minAngleFromNorth : 0;
        float maxAngle = options != null ? options.maxAngleFromNorth : 360;

        if (mainTarget)
        {
            referencePosition = m_playerPosition;
            minDist = m_minDistanceFromPlayer;
            maxDist = m_maxDistanceFromPlayer;
            minAngle = m_minAngleFromNorth;
            maxAngle = m_maxAngleFromNorth;
        }

        referencePosition.y = 0;
        float distance = Random.Range(minDist, maxDist);
        float radialAngle = Random.Range(minAngle, maxAngle);
        var radialRotation = Quaternion.Euler(0, radialAngle, 0);

        bool success = true;
  
        position = Vector3.forward * distance;
        position = radialRotation * position;
        position += referencePosition;

        if (objectsToAvoid == null)
            return true;

        for (int i = 0; i < objectsToAvoid.Count; i++)
        {
            var otherPosition = objectsToAvoid[i];
            otherPosition.y = 0;
            float separation = (position - otherPosition).magnitude;

            success = success && separation >= minSeparation;

            if (!success)
                break;
        }

        return success;
    }


    private bool TestAirPosition(PlaceableObjectAir testPlaceableObject, 
        PlacementOptionsAir options, bool mainTarget = false)
    {
        var testObject = testPlaceableObject.gameObject;
        var objectsToAvoid = mainTarget ? null : m_airObjectPositions;

        Vector3 trialPosition;
        bool success = GetTrialPosition(options, mainTarget, objectsToAvoid, out trialPosition);

        if (success)
        {
            var rotation = Quaternion.identity;
            float altitude = 0f;

            if (mainTarget)
            {
                float rotationY = Random.Range(0f, 360f);
                rotation = Quaternion.Euler(0, rotationY, 0);
                altitude = Random.Range(m_airObjectAltitudeMinMax.x, m_airObjectAltitudeMinMax.y);
            }
            else
            {
                var groundZero = new Vector2(m_groundZeroPosition.x, m_groundZeroPosition.z);
                var position = new Vector2(trialPosition.x, trialPosition.z);
                var direction = position - groundZero;
                float distance = direction.magnitude;
                var theta = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
                int flip = Random.Range(0, 2) * 2 - 1;
                rotation = Quaternion.Euler(0, theta + (90 * flip), 0);

                var flyingControlScript = testPlaceableObject.gameObject.GetComponent<FlyingControl>();

                float speed = flyingControlScript.ForwardSpeed;
                float turnRateRad = Mathf.Deg2Rad * flyingControlScript.turnRate;
                float sinThi = speed / (distance * turnRateRad);
                float bankAngleDeg = -flip * (Mathf.Abs(sinThi) > 1f 
                    ? Mathf.Sign(sinThi) * 90f 
                    : Mathf.Asin(sinThi) * Mathf.Rad2Deg);

                rotation *= Quaternion.Euler(0f, 0f, bankAngleDeg);

                altitude = Random.Range(options.minAltitude, options.maxAltitude);

                //print(string.Format("Speed: {0}, turn rate: {1}, distance: {2}, sinThi: {3}, bank angle: {4}", 
                //    speed, turnRate, distance, sinThi, bankAngle));
            }

            testObject.transform.rotation = rotation;

            trialPosition.y = altitude;
            testObject.transform.position = trialPosition;
            
            m_airObjectPositions.Add(trialPosition);
        }

        return success;
    }


    private bool TestGroundPosition(PlaceableObjectGround testPlaceableObject,
        PlacementOptionsGround options, bool mainTarget = false)
    {
        var testObject = testPlaceableObject.gameObject;
        var objectsToAvoid = mainTarget ? null : m_groundObjectPositions;

        Vector3 trialPosition;
        bool success = GetTrialPosition(options, 
            mainTarget, objectsToAvoid, out trialPosition);

        if (!success)
            return false;

        float rotationY = Random.Range(0f, 360f);
        var trialRotation = Quaternion.Euler(0, rotationY, 0);

        testObject.transform.position = trialPosition;

        var rigidbody = testObject.GetComponent<Rigidbody>();

        //var bounds = BoundsUtilities.OverallBounds(testPlaceableObject.gameObject);
        var bounds = testPlaceableObject.GetUnrotatedBounds();

        if (bounds != null)
        {
            var boundsData = FindTerrainHeightAtCorners(bounds.Value, trialPosition, trialRotation);

            float minHeight = options != null ? options.minHeight : m_groundObjectHeightMinMax.x;
            float maxHeight = options != null ? options.maxHeight : m_groundObjectHeightMinMax.y;
            float maxHeightDifference = testPlaceableObject.maxHeightDifference;

            float heightDifference = boundsData.HeightDifference();

			float y = rigidbody == null || rigidbody.useGravity == false
					? boundsData.minTerrainHeight
					: boundsData.maxTerrainHeight;// + 0.5f * maxHeightDifference;

            success = heightDifference <= maxHeightDifference  
                && boundsData.minTerrainHeight >= minHeight 
                && boundsData.maxTerrainHeight <= maxHeight;

            if (success)
            {
                y += boundsData.originAboveBase;
                trialPosition.y = y;

                testObject.transform.rotation = trialRotation;
                testObject.transform.position = trialPosition;

                if (!mainTarget)
                    m_groundObjectPositions.Add(trialPosition);

                if (m_showDebugSpheres)
                    AddDebugSpheres(boundsData, y, testObject.transform);
            }         
        }

        return success;
    }


    private bool TestWaterPosition(PlaceableObjectWater testPlaceableObject,
        PlacementOptionsWater options, bool mainTarget = false)
    {
        var testObject = testPlaceableObject.gameObject;
        var objectsToAvoid = mainTarget ? null : m_waterObjectPositions;

        Vector3 trialPosition;
        bool success = GetTrialPosition(options, 
            mainTarget, objectsToAvoid, out trialPosition);

        if (!success)
            return false;

        float rotationY = Random.Range(0f, 360f);
        var trialRotation = m_mainTargetTransform != null && options.alignWithMainTarget 
            ? m_mainTargetTransform.rotation
            : Quaternion.Euler(0, rotationY, 0);

        testObject.transform.position = trialPosition;

        //var bounds = BoundsUtilities.OverallBounds(testPlaceableObject.gameObject);
        var bounds = testPlaceableObject.GetUnrotatedBounds();

        if (bounds != null)
        {
            var boundsData = FindTerrainHeightAtCorners(bounds.Value, trialPosition, trialRotation);

            success = boundsData.maxTerrainHeight < -boundsData.originAboveBase;

            if (success)
            {
                trialPosition.y = 0;

                testObject.transform.rotation = trialRotation;
                testObject.transform.position = trialPosition;

                if (!mainTarget)
                    m_waterObjectPositions.Add(trialPosition);

                if (m_showDebugSpheres)
                    AddDebugSpheres(boundsData, 0, testObject.transform);
            }
        }

        return success;
    }


    private BoundsData FindTerrainHeightAtCorners(Bounds bounds,
        Vector3 trialPosition, Quaternion trialRotation)
    {
        float minY = bounds.min.y;
        float minX = bounds.min.x;
        float minZ = bounds.min.z;
        float maxX = bounds.max.x;
        float maxZ = bounds.max.z;

        var corner1 = new Vector3(minX, minY, minZ);
        var corner2 = new Vector3(minX, minY, maxZ);
        var corner3 = new Vector3(maxX, minY, minZ);
        var corner4 = new Vector3(maxX, minY, maxZ);
        var centre = new Vector3(0.5f * (minX + maxX), minY, 0.5f * (minZ + maxZ));

        // TODO: add side and centre sampling?
        var originToCorner1 = corner1 - trialPosition;
        var originToCorner2 = corner2 - trialPosition;
        var originToCorner3 = corner3 - trialPosition;
        var originToCorner4 = corner4 - trialPosition;
        var originToCentre = centre - trialPosition;

        float originAboveBase = trialPosition.y - bounds.min.y;

        originToCorner1 = trialRotation * originToCorner1;
        originToCorner2 = trialRotation * originToCorner2;
        originToCorner3 = trialRotation * originToCorner3;
        originToCorner4 = trialRotation * originToCorner4;
        originToCentre = trialRotation * originToCentre;

        corner1 = trialPosition + originToCorner1;
        corner2 = trialPosition + originToCorner2;
        corner3 = trialPosition + originToCorner3;
        corner4 = trialPosition + originToCorner4;
        centre = trialPosition + originToCentre;

        float terrainHeightCorner1 = m_mapGenerator.getRealHeight(corner1.x, corner1.z);
        float terrainHeightCorner2 = m_mapGenerator.getRealHeight(corner2.x, corner2.z);
        float terrainHeightCorner3 = m_mapGenerator.getRealHeight(corner3.x, corner3.z);
        float terrainHeightCorner4 = m_mapGenerator.getRealHeight(corner4.x, corner4.z);
        float terrainHeightCentre = m_mapGenerator.getRealHeight(centre.x, centre.z);

        float minTerrainHeight = Mathf.Min(terrainHeightCorner1, terrainHeightCorner2, terrainHeightCorner3, terrainHeightCorner4, terrainHeightCentre);
        float maxTerrainHeight = Mathf.Max(terrainHeightCorner1, terrainHeightCorner2, terrainHeightCorner3, terrainHeightCorner4, terrainHeightCentre);

        var boundsData = new BoundsData(corner1, corner2, corner3, corner4, centre, 
            minTerrainHeight, maxTerrainHeight, originAboveBase, terrainHeightCorner1, 
            terrainHeightCorner2, terrainHeightCorner3, terrainHeightCorner4, terrainHeightCentre);

        return boundsData;      
    }


    private void AddDebugSpheres(BoundsData boundsData, float y, Transform parent)
    {
        boundsData.corner1.y = y - boundsData.originAboveBase;
        boundsData.corner2.y = y - boundsData.originAboveBase;
        boundsData.corner3.y = y - boundsData.originAboveBase;
        boundsData.corner4.y = y - boundsData.originAboveBase;
        boundsData.centre.y = y - boundsData.originAboveBase;

        var sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere1.transform.position = boundsData.corner1;
        sphere2.transform.position = boundsData.corner2;
        sphere3.transform.position = boundsData.corner3;
        sphere4.transform.position = boundsData.corner4;
        sphere5.transform.position = boundsData.centre;

        //print(string.Format("Pos: {0}, height: {1}", corner1, terrainHeightCorner1));
        //print(string.Format("Pos: {0}, height: {1}", corner2, terrainHeightCorner2));
        //print(string.Format("Pos: {0}, height: {1}", corner3, terrainHeightCorner3));
        //print(string.Format("Pos: {0}, height: {1}", corner4, terrainHeightCorner4));

        var sphere6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere7 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere8 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere9 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var sphere10 = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere1.GetComponent<Collider>().enabled = false;
        sphere2.GetComponent<Collider>().enabled = false;
        sphere3.GetComponent<Collider>().enabled = false;
        sphere4.GetComponent<Collider>().enabled = false;
        sphere5.GetComponent<Collider>().enabled = false;
        sphere6.GetComponent<Collider>().enabled = false;
        sphere7.GetComponent<Collider>().enabled = false;
        sphere8.GetComponent<Collider>().enabled = false;
        sphere9.GetComponent<Collider>().enabled = false;
        sphere10.GetComponent<Collider>().enabled = false;

        boundsData.corner1.y = boundsData.terrainHeightCorner1;
        boundsData.corner2.y = boundsData.terrainHeightCorner2;
        boundsData.corner3.y = boundsData.terrainHeightCorner3;
        boundsData.corner4.y = boundsData.terrainHeightCorner4;
        boundsData.centre.y = boundsData.terrainHeightCentre;

        sphere6.transform.position = boundsData.corner1;
        sphere7.transform.position = boundsData.corner2;
        sphere8.transform.position = boundsData.corner3;
        sphere9.transform.position = boundsData.corner4;
        sphere10.transform.position = boundsData.centre;

        sphere1.transform.parent = parent;
        sphere2.transform.parent = parent;
        sphere3.transform.parent = parent;
        sphere4.transform.parent = parent;
        sphere5.transform.parent = parent;
        sphere6.transform.parent = parent;
        sphere7.transform.parent = parent;
        sphere8.transform.parent = parent;
        sphere9.transform.parent = parent;
        sphere10.transform.parent = parent;
    }
}
