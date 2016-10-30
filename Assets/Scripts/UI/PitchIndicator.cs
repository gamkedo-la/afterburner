using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PitchIndicator : MonoBehaviour {

    private Transform playerTransform;
    private RawImage pitchIndicatorRawImage;
    private float offsetZ;
    private float offsetY;


    // Use this for initialization
    void Start () {

        playerTransform = FindObjectOfType<PlayerFlyingInput>().transform;
        pitchIndicatorRawImage = GetComponent<RawImage>();

    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null)
            return;

        //Rotates Pitch Indicator to stay level with the horizon by inversing the rotation of the player 
        offsetZ = playerTransform.eulerAngles.z;

        //Inverses the value by getting the difference in degrees so that it can be used to set the rotation. (eg 350 degrees is converted to 10 degrees).
        offsetZ = 360 - offsetZ;

        transform.rotation = Quaternion.Euler(0, 0, offsetZ);

        offsetY = playerTransform.eulerAngles.x;

        //This converts the euler angle value stored in offsetX to a value between 0 and 180.  0 is down, 90 is forward, 180 is up.
        if(offsetY <= 90)
        {
            offsetY -= 90;
            offsetY = Mathf.Abs(offsetY);
        }else if(offsetY >= 270)
        {
            offsetY -= 360;
            offsetY = Mathf.Abs(offsetY);
            offsetY += 90;
        }

        //Divide by 180 because uvRect offset uses a value between 0 and 1.
        offsetY /= 180;

        //Adds 0.5 to orient the image correctly so that the horizon line is on the horizon
        offsetY += 0.5f;

        //Keep all the values the same except for Y
        pitchIndicatorRawImage.uvRect = new Rect(pitchIndicatorRawImage.uvRect.x, offsetY, pitchIndicatorRawImage.uvRect.width, pitchIndicatorRawImage.uvRect.height);

    }
}
