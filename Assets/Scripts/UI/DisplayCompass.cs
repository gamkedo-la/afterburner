using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DisplayCompass : MonoBehaviour
{
    private RawImage compassRawImage;
    private Text compassText;
    private Transform playerTransform;
    private float offsetX;


	void Start()
    {
        compassRawImage = GameObject.Find("Compass Graphic").GetComponent<RawImage>();
        compassText = GameObject.Find("Compass Text").GetComponent<Text>();
        playerTransform = FindObjectOfType<PlayerFlyingInput>().transform;

        compassText.text = Mathf.Floor(playerTransform.eulerAngles.y).ToString();
    }


    void Update()
    {
        if (playerTransform == null)
            return;

        compassText.text = Mathf.Floor(playerTransform.eulerAngles.y).ToString(); 

        offsetX = playerTransform.eulerAngles.y;

        //Divide by 360 because uvRect range is from 0 to 1
        offsetX /= 360;

        //Keep all the values the same except for X
        compassRawImage.uvRect = new Rect(offsetX, compassRawImage.uvRect.y, compassRawImage.uvRect.width, compassRawImage.uvRect.height);
    }
}
