using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisplayHealth : MonoBehaviour {

    PlayerHealth playerHealth;

    private Text healthPercentage;


	// Use this for initialization
	void Start () {
        playerHealth = FindObjectOfType<PlayerHealth>();



        healthPercentage = GameObject.Find("Health Percentage").GetComponent<Text>();

    }
	
	// Update is called once per frame
	void Update () {

        healthPercentage.text = (((float) playerHealth.CurrentHealth / playerHealth.StartingHealth) * 100).ToString() + "%";

    }
}
