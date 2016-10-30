using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisplayAltitude : MonoBehaviour {

    //Convert speed to more useful unit, like mph or kph
    public float altitudeMultiplier;

    //Altitude Text
    private float altitude;
    private Text altitudeText;
    private Transform playerTransform;

    //Altitude Line Indicator
    private RawImage altitudeIndicatorRawImage;
    private float offsetY;


    // Use this for initialization
    void Start () {

        altitudeText = GameObject.Find("Altitude Text").GetComponent<Text>();
        playerTransform = FindObjectOfType<PlayerFlyingInput>().transform;
        altitude = Mathf.Ceil(playerTransform.position.y * altitudeMultiplier);
        altitudeIndicatorRawImage = GameObject.Find("Altitude Line Indicator").GetComponent<RawImage>();

    }
	
	// Update is called once per frame
	void Update () {

        if (playerTransform == null)
            return;

        //Altitude Text
        altitude = Mathf.Ceil(playerTransform.position.y * altitudeMultiplier);
        altitudeText.text = altitude.ToString();

        offsetY = altitude;

        //The main markers go in increments of 100
        offsetY /= 100;

        //The graphic for the line indicator has 30 marks on it with 3 "main" marks that represent units of 100, so this gives you 3 main marks per offset
        offsetY /= 3;

        //Keep all the values the same except for Y
        altitudeIndicatorRawImage.uvRect = new Rect(altitudeIndicatorRawImage.uvRect.x, offsetY, altitudeIndicatorRawImage.uvRect.width, altitudeIndicatorRawImage.uvRect.height);

    }

}
