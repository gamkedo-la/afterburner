using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisplaySpeed : MonoBehaviour {

    private Slider speedSlider;
    private float speed;

    //Text Display
    private Text speedText;    
    //Convert speed to more useful unit, like mph or kph
    public float speedMultiplier;

    //Line Indicator
    private RawImage speedIndicatorRawImage;
    private float offsetY;


	// Use this for initialization
	void Start () {

        //Slider
        speedSlider = GameObject.Find("Speed Slider").GetComponent<Slider>();

        //Text Display
        speedText = GameObject.Find("Speed Text").GetComponent<Text>();

        //Line Indicator
        speedIndicatorRawImage = GameObject.Find("Speed Line Indicator").GetComponent<RawImage>();

    }
	
	// Update is called once per frame
	void Update () {


        //Text Display
        speed = Mathf.Ceil(speedSlider.value * speedMultiplier);
        speedText.text = speed.ToString();


        //Line Indicator
        offsetY = speed;

        //The main markers go in increments of 100
        offsetY /= 100;

        //The graphic for the line indicator has 30 marks on it with 3 "main" marks that represent units of 100, so this gives you 3 main marks per offset
        offsetY /= 3;

        //Keep all the values the same except for Y
        speedIndicatorRawImage.uvRect = new Rect(speedIndicatorRawImage.uvRect.x, offsetY, speedIndicatorRawImage.uvRect.width, speedIndicatorRawImage.uvRect.height);


    }
}
