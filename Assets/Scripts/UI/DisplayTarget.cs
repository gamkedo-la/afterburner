using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


public class DisplayTarget : MonoBehaviour {

    private static readonly string Enemy = "Enemy";
    private static readonly string Target = "Target";

    //Arrays
    private List<EnemyHealth> enemies;
    private List<GameObject> radarDots;

    private Transform playerTransform;

    private Text targetName;
    private Text targetDistance;
    private Text targetHealth;

    private EnemyHealth enemyHealth;

    private int targetIndex;
    private Vector3 direction;

    private Camera targetCamera;

    private Color targetBorderColor;

    private Image targetBorderTop;
    private Image targetBorderBottom;

    private GameObject targetSelectIcon;

    private bool initialised;

    private bool closestTargetAxisInUse;
    private bool cycleTargetAxisInUse;

    private AudioSource targetSelectAudioSource;

    private EnemyHealth m_currentTarget;

    
    void Start()
    {
        Initialise();           
    }


    public void Initialise()
    {
        if (initialised)
            return;

        targetIndex = 0;

        var radar = FindObjectOfType<DisplayRadar>();
        radar.Initialise();

        enemies = radar.returnEnemyList();
        radarDots = radar.returnRadarDotList();

        targetName = GameObject.Find("Target Name").GetComponent<Text>();
        targetDistance = GameObject.Find("Target Distance").GetComponent<Text>();
        targetHealth = GameObject.Find("Target Health").GetComponent<Text>();

        playerTransform = FindObjectOfType<PlayerFlyingInput>().GetComponent<Transform>();
        targetCamera = GameObject.Find("Target Camera").GetComponent<Camera>();

        targetBorderTop = GameObject.Find("Target Border Top").GetComponent<Image>();
        targetBorderBottom = GameObject.Find("Target Border Bottom").GetComponent<Image>();

        updateBorderColor();

        targetSelectIcon = GameObject.Find("Target Select Icon");
		if(targetSelectIcon == null) {
			targetSelectIcon = GameObject.Find("Target Icon");
		}

        targetSelectAudioSource = GetComponent<AudioSource>();

        initialised = true;
    }


    // Update is called once per frame
    void LateUpdate () {

        if (playerTransform == null)
            return;

        if (enemies.Count == 0)
        {
            targetName.text = "No Target";
            targetDistance.text = " ";
            targetHealth.text = " ";
            targetBorderTop.color = Color.white;
            targetBorderBottom.color = Color.white;
            targetSelectIcon.SetActive(false);
            return;
        }

        if (targetIndex > enemies.Count - 1)
        {
            targetIndex = 0;
            //Debug.Log("Reset To Zero");
        }

        var enemy = enemies[targetIndex];

        if (enemy != null && !enemy.IsDead)
        {   
            //Move current target to Target layer which is what the culling mask of Target Camera is set to
            if (enemy != m_currentTarget)
            {
                m_currentTarget = enemy;
                ChangeLayersRecursively(enemies[targetIndex].transform, Enemy, Target);
            }

            //Gets object bounds of mesh and then chooses whichever is largest, x y or z.
            var overallBounds = enemies[targetIndex].GetComponent<PlaceableObject>().GetUnrotatedBounds();
            var largestBound = 0f;
            if (overallBounds.Value.size.x > overallBounds.Value.size.y 
                && overallBounds.Value.size.x > overallBounds.Value.size.z)
            {
                largestBound = overallBounds.Value.size.x;
            }
            else if (overallBounds.Value.size.y > overallBounds.Value.size.x
                && overallBounds.Value.size.y > overallBounds.Value.size.z)
            {
                largestBound = overallBounds.Value.size.y;
            }
            else
            {
                largestBound = overallBounds.Value.size.z;
            }

            //Debug.Log("Overall Bounds Max: " + largestBound);
            targetCamera.orthographicSize = (largestBound / 2f) + 5.5f;

            targetSelectIcon.SetActive(true);
            updateBorderColor();

            targetName.text = enemies[targetIndex].name;
            direction = enemies[targetIndex].transform.position - playerTransform.position;
            //targetSelectIcon.transform.SetParent(radarDots[targetIndex].transform);
            targetSelectIcon.transform.position = radarDots[targetIndex].transform.position;

            //Makes sure whatever colored dot you are over is rendered on top
            radarDots[targetIndex].transform.SetAsLastSibling();

            targetDistance.text = "Dist: " + (direction.magnitude / 100).ToString("f1");

            enemyHealth = enemies[targetIndex].GetComponent<EnemyHealth>();
            targetHealth.text = "Hull: " + Mathf.Ceil(((float) enemyHealth.CurrentHealth / enemyHealth.StartingHealth) * 100) + "%";

            cameraTrackTarget();
        }
        else
        {
            Destroy(radarDots[targetIndex]);
            enemies.Remove(enemy);
            radarDots.Remove(radarDots[targetIndex]);
            //Debug.Log("Enemies Length: " + enemies.Count);
        }

        float cycleTargetInput = Input.GetAxisRaw("Cycle Target");
        float closestTargetInput = Input.GetAxisRaw("Closest Target");

        if (cycleTargetInput == 0)
            cycleTargetAxisInUse = false;

        if (closestTargetInput == 0)
            closestTargetAxisInUse = false;

        if (!cycleTargetAxisInUse && cycleTargetInput == 1)
        {
            cycleTargetAxisInUse = true;
  
            targetSelectAudioSource.Play();

            //Reset Previous Target back to Enemy Layer
            ChangeLayersRecursively(enemies[targetIndex].transform, Target, Enemy);

            targetIndex++;

            if (targetIndex >= enemies.Count)
            {
                targetIndex = 0;
            }

            if (enemies[targetIndex].tag == "Enemy Air")
            {
                //Debug.Log("Enemy Air");
            }

            //updateBorderColor();
        }
        else if (!cycleTargetAxisInUse && cycleTargetInput == -1)
        {
            cycleTargetAxisInUse = true;

            targetSelectAudioSource.Play();

            //Reset Previous Target back to Enemy Layer
            ChangeLayersRecursively(enemies[targetIndex].transform, Target, Enemy);

            targetIndex--;

            if (targetIndex < 0)
            {
                targetIndex = enemies.Count - 1;
            }

            //updateBorderColor();
        }
        else if (!closestTargetAxisInUse && closestTargetInput == 1)
        {
            closestTargetAxisInUse = true;

            targetSelectAudioSource.Play();

            //Reset Previous Target back to Enemy Layer
            ChangeLayersRecursively(enemies[targetIndex].transform, Target, Enemy);

			// previous approach, selected physically nearest irrespective of facing
            /* float lowestMagnitude = direction.magnitude;

            for (int key = 0; key < enemies.Count; key++)
            {
                direction = enemies[key].transform.position - playerTransform.position;
                if (direction.magnitude < lowestMagnitude)
                {
                    lowestMagnitude = direction.magnitude;
                    targetIndex = key;
                }
            }*/
			// new approach, selects target most directly in front of nose cone
			float nearestAngle = 180.0f;
			for (int key = 0; key < enemies.Count; key++)
			{
				float nextAng = 
					Quaternion.Angle(
						Quaternion.LookRotation(enemies[key].transform.position - playerTransform.position),
						Quaternion.LookRotation(playerTransform.forward));
				if (nextAng < nearestAngle)
				{
					nearestAngle = nextAng;
					targetIndex = key;
				}
			}		

            //Set new Target to Target Layer
            ChangeLayersRecursively(enemies[targetIndex].transform, Enemy, Target);
            //updateBorderColor();
        }

        //Debug.Log("Target Index: " + targetIndex);
    }


    void cameraTrackTarget()
    {
        var target = enemies[targetIndex].gameObject;
        var overallBounds = BoundsUtilities.OverallBounds(target);
        Vector3 centre = overallBounds.Value.center;

        targetCamera.transform.position = centre - (direction.normalized * 15.0f);
        targetCamera.transform.LookAt(centre, Camera.main.transform.up);
        var angles = targetCamera.transform.rotation.eulerAngles;
        angles.z = 0;
        targetCamera.transform.rotation = Quaternion.Euler(angles);
    }


    void updateBorderColor()
    {
        targetBorderColor = radarDots[targetIndex].GetComponent<Image>().color;
        targetBorderTop.color = targetBorderColor;
        targetBorderBottom.color = targetBorderColor;
    }


    public GameObject returnCurrentTarget()
    {
        if(targetIndex < enemies.Count)
            return enemies[targetIndex].gameObject;
        else
            return null;
    }


    void ChangeLayersRecursively(Transform trans, string from, string to)
    {
        if (LayerMask.LayerToName(trans.gameObject.layer) == from)
            trans.gameObject.layer = LayerMask.NameToLayer(to);

        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, from, to);
        }
    }


    private void EnemyDestroyed(Transform transform)
    {
        ChangeLayersRecursively(transform, Target, Enemy);
    }


    void OnEnable()
    {
        EventManager.StartListening(TransformEventName.EnemyDead, EnemyDestroyed);
    }


    void OnDisable()
    {
        EventManager.StopListening(TransformEventName.EnemyDead, EnemyDestroyed);
    }
}
