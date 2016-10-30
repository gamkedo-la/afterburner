using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class DisplayRadar : MonoBehaviour
{


    public float radarRadius = 250f;
    private Transform frontRadarPanelTransform;
    private Transform rearRadarPanelTransform;
    private Transform playerTransform;

    private float yawDiff = 0;
    private float pitchDiff = 0;

    private float blipFromCenterScale;
    private float polarPtOnRadarX;
    private float polarPtOnRadarY;

    //private GameObject[] enemies;
    //private GameObject[] radarDots;
    private List<EnemyHealth> enemyList = new List<EnemyHealth>();
    private List<GameObject> radarDotList = new List<GameObject>();

    public GameObject radarBlip;

    private Vector3 targetRelative;
    private float angleAngToBlip;

    public Color enemyAirColor;
    public Color enemySeaColor;
    public Color enemyLandColor;
    public Color defaultTargetColor;

    private bool initialised;


    // Use this for initialization
    void Start()
    {
        Initialise();
    }


    public void Initialise()
    {
        if (initialised)
            return;

        //blip = GameObject.Find("Radar Blip").GetComponent<RectTransform>();
        frontRadarPanelTransform = GameObject.Find("Front Radar Panel").GetComponent<RectTransform>();
        rearRadarPanelTransform = GameObject.Find("Rear Radar Panel").GetComponent<RectTransform>();

        playerTransform = FindObjectOfType<PlayerFlyingInput>().transform;

        //populate enemies array with all enemies tagged with Enemy
        //GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        //print(enemies.Length + " enemies found");
        //radarDots = new GameObject[enemies.Length];

        var allEnemies = new List<GameObject>();
        GameObject[] enemyAir = GameObject.FindGameObjectsWithTag("Enemy Air");
        allEnemies.AddRange(enemyAir);
        GameObject[] enemyLand = GameObject.FindGameObjectsWithTag("Enemy Land");
        allEnemies.AddRange(enemyLand);
        GameObject[] enemySea = GameObject.FindGameObjectsWithTag("Enemy Sea");
        allEnemies.AddRange(enemySea);

        for (int i = 0; i < allEnemies.Count; i++)
        {
            var enemyHealth = allEnemies[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyList.Add(enemyHealth);
        }

        //Instantiate radar blips with the appropriate color for each enemy and assign them to a list
        GameObject tempRadarDot;

        for (int key = 0; key < enemyList.Count; key++)
        {
            tempRadarDot = (GameObject) Instantiate(radarBlip, frontRadarPanelTransform.transform.position, frontRadarPanelTransform.transform.rotation);
            tempRadarDot.transform.SetParent(frontRadarPanelTransform);
            tempRadarDot.transform.localScale = new Vector3(0.5F, 0.5f, 1);

            if (enemyList[key].tag == "Enemy Air")
                tempRadarDot.GetComponent<Image>().color = enemyAirColor;
            else if (enemyList[key].tag == "Enemy Sea")
                tempRadarDot.GetComponent<Image>().color = enemySeaColor;
            else if (enemyList[key].tag == "Enemy Land")
                tempRadarDot.GetComponent<Image>().color = enemyLandColor;
            else
                tempRadarDot.GetComponent<Image>().color = defaultTargetColor;

            radarDotList.Add(tempRadarDot);
        }

        //Debug.Log("Radar Dots List : " + radarDotList.Count + "       Enemy count: " + enemyList.Count);

        initialised = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null)
            return;

        for (int key = 0; key < enemyList.Count; key++)
        {
            var enemy = enemyList[key];
            //If enemy hasn't been destroyed, update radar dot position, otherwise destroy
            if (enemy != null && !enemy.IsDead)
            {
                targetRelative = playerTransform.InverseTransformDirection(enemyList[key].transform.position - playerTransform.position);

                CalculatePolarPoints();

                if (yawDiff >= -90 && yawDiff <= 90 && pitchDiff >= -90 && pitchDiff <= 90)
                {

                    radarDotList[key].GetComponent<RectTransform>().SetParent(frontRadarPanelTransform);
                    radarDotList[key].GetComponent<RectTransform>().anchoredPosition = new Vector2(polarPtOnRadarX, polarPtOnRadarY);
                }
                else
                {

                    targetRelative = playerTransform.InverseTransformDirection(playerTransform.position - enemyList[key].transform.position);

                    CalculatePolarPoints();

                    //Use negatives to mirror the view for the Rear Radar
                    radarDotList[key].GetComponent<RectTransform>().SetParent(rearRadarPanelTransform);
                    radarDotList[key].GetComponent<RectTransform>().anchoredPosition = new Vector2(-polarPtOnRadarX, -polarPtOnRadarY);
                }
            }
            else
            {        
                Destroy(radarDotList[key]);
                enemyList.Remove(enemy);
                radarDotList.Remove(radarDotList[key]);
                //Debug.Log("Enemy Object Destroy");
            }        
        }
    }

    private void CalculatePolarPoints()
    {

        yawDiff = Mathf.Atan2(targetRelative.x, targetRelative.z) * Mathf.Rad2Deg;
        pitchDiff = Mathf.Atan2(targetRelative.y, targetRelative.z) * Mathf.Rad2Deg;

        //Debug.Log("yawDiff = " + yawDiff + "    pitchDiff = " + pitchDiff);

        blipFromCenterScale = Mathf.Max(Mathf.Abs(pitchDiff), Mathf.Abs(yawDiff)) / 90.0f;

        angleAngToBlip = Mathf.Atan2(pitchDiff, yawDiff);

        polarPtOnRadarX = blipFromCenterScale * radarRadius * Mathf.Cos(angleAngToBlip);
        polarPtOnRadarY = blipFromCenterScale * radarRadius * Mathf.Sin(angleAngToBlip);

    }

    public List<EnemyHealth> returnEnemyList()
    {
        return enemyList;
    }

    public List<GameObject> returnRadarDotList()
    {
        return radarDotList;
    }

}
