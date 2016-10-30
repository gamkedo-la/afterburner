using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayTargetBox : MonoBehaviour
{
    public DisplayTarget displayTarget;
    //Leaves room around the edges of the target
    public float scaleMultiplier = 1.0f;

    public float minTargetBoxSize;

    private Image targetBox;
	private RectTransform targetBoxRectTransform;

	private GameObject target;
   

    // Use this for initialization
    void Start ()
	{
        targetBox = GameObject.Find("Target Box").GetComponent<Image>();
        targetBoxRectTransform = targetBox.GetComponent<RectTransform>();
        displayTarget.Initialise();
    }
	
	// Update is called once per frame
	void LateUpdate ()
	{

        if (displayTarget.returnCurrentTarget() != null)
            target = displayTarget.returnCurrentTarget();

        //Checks to make sure target exists and is in front of you
        if(target != null && Vector3.Dot(target.transform.position - Camera.main.transform.position, Camera.main.transform.forward) > 0)
        {

            targetBox.enabled = true;
            targetBox.transform.position = Camera.main.WorldToScreenPoint(target.transform.position);
            Rect worldBounds = GUIRectWithObject(target);

            //Sets minimum size of target box so that it does not dissapear on the horizon
            if (worldBounds.width < minTargetBoxSize)
                worldBounds.width = minTargetBoxSize;

            if (worldBounds.height < minTargetBoxSize)
                worldBounds.height = minTargetBoxSize;

            //Takes whichever value is larger, height or width, and sets the other one to it so that the box is always square
            if (worldBounds.height > worldBounds.width)
                worldBounds.width = worldBounds.height;
            else{
                worldBounds.height = worldBounds.width;
            }

            targetBoxRectTransform.sizeDelta = new Vector2(worldBounds.width, worldBounds.height) * scaleMultiplier;

            //Sets Z to zero to prevent the target box dissapear on the horizon
            targetBoxRectTransform.anchoredPosition3D = new Vector3(targetBoxRectTransform.anchoredPosition.x, targetBoxRectTransform.anchoredPosition.y, 0);
        }
        else
        {
            //prevents target from showing up when it is offscreen
            targetBox.enabled = false;
        }
    }

    //Takes 
	public static Rect GUIRectWithObject(GameObject go)
	{
        //Combines all renderer bounds together for multipart objects
        var overallBounds = BoundsUtilities.OverallBounds(go);

        Vector3 cen = overallBounds.Value.center;
        Vector3 ext = overallBounds.Value.extents;

        Vector2[] extentPoints = new Vector2[8]
		{
			Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
			
			Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
			Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
		};
		
		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];
		
		foreach(Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}
		
		return new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
	}
}
