using UnityEngine;
using System.Collections;

public delegate void RaycastInteractionHandler(RaycastInteractionInfo r);

public class RaycastInteractionInfo
{
    float rayAngle;
    bool isActive; //true means we are colliding with something
    RaycastHit hitInfo; //info on what the ray collided with
    bool bForward;

    public RaycastInteractionInfo()
    {
        rayAngle = 0.0f;
        isActive = false;
        hitInfo = new RaycastHit();
        bForward = false;
    }

    public RaycastInteractionInfo(float _rayAngle, bool _isActive, RaycastHit _hitInfo, bool _bForward)
    {
        rayAngle = _rayAngle;
        isActive = _isActive;
        hitInfo = _hitInfo;
        bForward = _bForward;
    }

    public void UpdateInfo(float _rayAngle, bool _isActive, RaycastHit _hitInfo, bool _bForward)
    {
        rayAngle = _rayAngle;
        isActive = _isActive;
        hitInfo = _hitInfo;
        bForward = _bForward;
    }
}

public class RaycastWhiskers : MonoBehaviour {

    public event RaycastInteractionHandler RayChanged;

    public float rayLength = 2.0f;
    public float whiskerRadius = 2.0f;
    public int radialWhiskerCount = 4;
    public bool forwardWhisker = true; //draws the whisker on forward vector
    public bool displayDebug = false;
    public LayerMask ignoreMask = -1;
    protected bool[] rayStore;
    protected bool rayStoreChanged;
	// Use this for initialization
	void Start () {
        ResetRayStore();
	}

    protected void ResetRayStore()
    {
        rayStore = new bool[radialWhiskerCount + (forwardWhisker ? 1 : 0)];
        rayStoreChanged = true;
    }
	
	// Update is called once per frame
	void Update () {
        RaycastWhisker_Update();
    }

    protected void RaycastWhisker_Update()
    {
        if (rayStore.Length != radialWhiskerCount + (forwardWhisker ? 1 : 0))
            ResetRayStore();

        RadialWhiskers();

        if (forwardWhisker)
        {
            ForwardWhisker();
        }
    }

    public virtual void OnRaycastChange(RaycastInteractionInfo info)
    {
        rayStoreChanged = true;
        if (RayChanged != null)
        {
            RayChanged(info);
        }
    }

    protected void RadialWhiskers()
    {
        Vector3 normal = transform.forward.normalized;
        Vector3 perp1 = transform.up.normalized;        //vector describing the plane of the circle
        Vector3 perp2 = transform.right.normalized; //vector describing the plane of the circle

        for (int i = 0; i < radialWhiskerCount; i++)
        {
            Color debugRayColor = Color.blue;
            float angleRads = i * (Mathf.PI * 2 / radialWhiskerCount);
            Vector3 forwardCircumferencePoint = GetCircumferencePoint(normal, perp1, perp2, transform.position + (transform.forward.normalized * rayLength), whiskerRadius, angleRads);
            RaycastHit hit;
            Ray nextRay = new Ray(transform.position, forwardCircumferencePoint - transform.position);
            
            if (Physics.Raycast(nextRay, out hit, rayLength, ignoreMask))
            {
                if(!rayStore[i])
                {
                    rayStore[i] = true;
                    OnRaycastChange(new RaycastInteractionInfo(angleRads, true, hit, false));
                }
                debugRayColor = Color.green;
            }
            else
            {
                if (rayStore[i])
                {
                    rayStore[i] = false;
                    OnRaycastChange(new RaycastInteractionInfo(angleRads, false, hit, false));
                }
            }
            if (displayDebug)
                Debug.DrawRay(transform.position, nextRay.direction * rayLength, debugRayColor);
        }
    }

    protected void ForwardWhisker()
    {
        Color debugRayColor = Color.blue;
        RaycastHit hit;
        Ray nextRay = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(nextRay, out hit, rayLength, ignoreMask))
        {
            if (rayStore.Length > 0 && !rayStore[rayStore.Length - 1])
            {
                rayStore[rayStore.Length - 1] = true;
                OnRaycastChange(new RaycastInteractionInfo(0.0f, true, hit, true));
            }
            debugRayColor = Color.green;
        }
        else
        {
            if (rayStore.Length > 0 && rayStore[rayStore.Length - 1])
            {
                rayStore[rayStore.Length - 1] = false;
                OnRaycastChange(new RaycastInteractionInfo(0.0f, false, hit, true));
            }
        }
        if (displayDebug)
            Debug.DrawRay(transform.position, nextRay.direction * rayLength, debugRayColor);
    }

    protected Vector3 GetCircumferencePoint(Vector3 normal, Vector3 perp1, Vector3 perp2, Vector3 center, float radius, float angleRads)
    {
        Vector3 retVect = Vector3.zero;
        
        retVect = Mathf.Sin(angleRads) * perp1 + Mathf.Cos(angleRads) * perp2;
        retVect *= radius; //adjust the radius of the point (scale it)
        retVect += center; //adjust to the current world position
        return retVect;
    }

}
