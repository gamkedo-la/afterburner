using UnityEngine;

using System.Collections.Generic;

public class CloudGenerator : MonoBehaviour
{
    [SerializeField] CloudProperties[] m_cloudProperties;

    [Header("Overall distribution")]
    [SerializeField] float m_maxDistance = 1500f;

    private GameObject m_cloudParent;
    private float m_maxDistanceSq;
    private Vector2 m_direction;
    private Transform m_mainCamera;
    private Vector2 m_origin;
    private Transform[] m_cloudTransforms;


    public float MaxDistance
    {
        get { return m_maxDistance; }
    }


	void Start () 
	{
        m_cloudParent = gameObject;
        var cloudTransforms = new List<Transform>();
        var cloudPositions = new List<Vector2>();

		m_mainCamera = Camera.main.transform;
		var cameraOrigin = m_mainCamera.transform.position;
		m_origin.Set(cameraOrigin.x, cameraOrigin.z);

        m_maxDistanceSq = m_maxDistance * m_maxDistance;

        for (int j = 0; j < m_cloudProperties.Length; j++)
        {
            var cloud = m_cloudProperties[j];

            int numberOfClouds = cloud.numberOfClouds;
            
            for (int i = 0; i < numberOfClouds; i++)
            {
                var position = GetSpawnPosition(cloud.minSeparation, cloudPositions);

                if (!position.HasValue)
                    continue;

                var newCloud = Instantiate(cloud.cloudPrefab);

                float y = Random.Range(cloud.minAltitude, cloud.maxAltitude);
                newCloud.transform.position = new Vector3(position.Value.x, y, position.Value.y);

                float scale = Random.Range(cloud.minScale, cloud.maxScale);
                newCloud.transform.localScale = new Vector3(scale, scale, scale);

                var rotation = Random.Range(0f, 356f) * cloud.rotationAxis;
                newCloud.transform.Rotate(rotation);

                cloudTransforms.Add(newCloud.transform);
                cloudPositions.Add(position.Value);

                if (m_cloudParent != null)
                    newCloud.transform.parent = m_cloudParent.transform;
            }
        }

        m_cloudTransforms = cloudTransforms.ToArray();
	}


	private Vector2? GetSpawnPosition(float minSeparation, List<Vector2> cloudPositions) 
	{
		Vector2 spawnPosition = new Vector2();
		float startTime = Time.realtimeSinceStartup;
		bool test = false;
		
		while (test == false) 
		{
			test = true;
			spawnPosition = Random.insideUnitCircle * m_maxDistance + m_origin;
			
			for (int i = 0; i < cloudPositions.Count; i++)
			{
                var cloud = cloudPositions[i];
				float distance = Vector2.Distance(cloud, spawnPosition);
				
				if (distance < minSeparation)
					test = false;
			}
			
			if (Time.realtimeSinceStartup - startTime > 0.1f) 
			{
				print("Time out placing Cloud!");
				return null;
			}
		}
		
		return spawnPosition;
	}


	void Update () 
	{
		for (int i = 0; i < m_cloudTransforms.Length; i++)
		{
            var cloud = m_cloudTransforms[i];
			cloud.Translate(Time.deltaTime * WeatherController.windSpeed, Space.World);

			var direction3 = m_mainCamera.position - cloud.position;

			m_direction.Set(direction3.x, direction3.z);

			float distanceSq = m_direction.sqrMagnitude;

            if (distanceSq > m_maxDistanceSq)
			{
				m_direction.Normalize();
				float newX = cloud.position.x + (m_direction.x * m_maxDistance * 2f);
				float newY = cloud.position.y;
				float newZ = cloud.position.z + (m_direction.y * m_maxDistance * 2f);

				cloud.position = new Vector3(newX, newY, newZ);
			}
		}
	}
}
