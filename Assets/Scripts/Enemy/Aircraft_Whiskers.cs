using UnityEngine;
using System.Collections;

public delegate void AircraftWhiskerFeedback(AircraftWhiskerInfo r);

public class AircraftWhiskerInfo
{
    public Vector3 safeVector;
    public bool isActive; //true means we are colliding with something
    public bool isRoll;
    public AircraftWhiskerInfo(Vector3 _safeVector, bool _isActive, bool _isRoll)
    {
        safeVector = _safeVector;
        isActive = _isActive;
        isRoll = _isRoll;
    }
}

public class Aircraft_Whiskers : RaycastWhiskers {
    protected Vector3 targetSafeVector;
    protected bool isRollVector;
    bool bWhiskerCollision = false;
    public event AircraftWhiskerFeedback WhiskerCollisionSet;
    bool bWaitingOnCoroutine;
    // Use this for initialization
    void Start () {
        isRollVector = false;
        bWhiskerCollision = false;
        targetSafeVector = Vector3.zero;
        ResetRayStore();
	}
	
	// Update is called once per frame
	void Update () {
        RaycastWhisker_Update();
        
        if(bWhiskerCollision)
            DrawSafeVector();
	}

    public override void OnRaycastChange(RaycastInteractionInfo info)
    {
        //a ray has changed
        if(!bWaitingOnCoroutine)
        {
            bWaitingOnCoroutine = true;
            StartCoroutine(WaitForAllRayChanges());
        }
    }

    IEnumerator WaitForAllRayChanges()
    {
        yield return new WaitForEndOfFrame();
        bWhiskerCollision = AreWhiskersColliding();

        if(bWhiskerCollision)
            targetSafeVector = GenerateSafeVector();
        
        OnWhiskerCollisionSet();
        bWaitingOnCoroutine = false;
        yield return null;
    }

    public virtual void OnWhiskerCollisionSet()
    {
        
        //print("whisker set broadcast");
        if (WhiskerCollisionSet != null)
        {
            
            WhiskerCollisionSet(new AircraftWhiskerInfo(targetSafeVector, bWhiskerCollision, isRollVector));
        }
    }

    bool AreWhiskersColliding()
    {
        for(int i = 0; i < rayStore.Length; i++)
        {
            if (rayStore[i])
                return true;
        }
        return false;
    }

    Vector3 GenerateSafeVector()
    {
        Vector3 normal = transform.forward.normalized;
        Vector3 perp1 = transform.up.normalized;        //vector describing the plane of the circle
        Vector3 perp2 = transform.right.normalized; //vector describing the plane of the circle

        bool bNoRadialCollisions = false;
        ArrayList safeAverages = new ArrayList();
        float angleIncrements = (Mathf.PI * 2 / radialWhiskerCount);
        Vector3 averageVector = Vector3.zero;
        //group false values together into an average
        for(int i = 0; i < radialWhiskerCount; i++)
        {
            if(!rayStore[i])
            {
                Vector3 safeVector = GetCircumferencePoint(normal, perp1, perp2, transform.position, whiskerRadius, i * angleIncrements) - transform.position;
                safeVector.Normalize(); //normalize it before adding to average
                //this whisker is safe, add it to average
                if (averageVector == Vector3.zero)
                    averageVector = safeVector;
                else
                    averageVector += safeVector; //add the vectors together (magnitude of the added vectors will determine safest vector later
               // print("added safevector to average : " + safeVector.ToString());

                if(i == radialWhiskerCount-1)
                {
                    //on the last whisker, consider connecting the last whisker group to the first whisker group
                    if(!rayStore[0] && i != 0)
                    {
                        //last whisker is safe, as well as the first whisker, they are the same group add them together
                        if(safeAverages.Count > 0)
                        {
                            safeAverages[0] = ((Vector3)safeAverages[0]) + averageVector;
                        }
                        else
                        {
                            //no averages stored yet, all radial whiskers must be safe then
                            safeAverages.Add(averageVector);
                            bNoRadialCollisions = true;
                        }
                    }
                    else
                    {
                        //store the average away
                        safeAverages.Add(averageVector);
                    }
                }
            }
            else
            {
                //this whisker is colliding with something, if there is a current safeVector being averaged, store it away
                if(averageVector != Vector3.zero)
                {
                    //print("stored average");
                    safeAverages.Add(averageVector);
                    averageVector = Vector3.zero;
                }
            }
        }

        //print("averageStored vectors: " + safeAverages.Count.ToString());
        Vector3 bestAverage = Vector3.zero;

        //have the average vectors stored, now loop through and find the largest magnitude to set as the safe vector
        for(int i = 0; i < safeAverages.Count;i++)
        {
            if(((Vector3)safeAverages[i]).magnitude > bestAverage.magnitude)
            {
                bestAverage = (Vector3)safeAverages[i];
            }
        }

        if(forwardWhisker)
        {
            //there is a forward whisker

            //check if the forward whisker is hitting something, the forward whisker is at the end of the raystore
            if(rayStore[rayStore.Length-1])
            {
                //forward whisker is hitting something, if there are no radial collisions, return the up vector as safe

                //if there are radial collisions, do nothing

                if(bNoRadialCollisions)
                {
                    bestAverage = transform.up;
                }
            }
        }

        //if all whiskers are colliding, just pull up
        if (bestAverage == Vector3.zero)
            bestAverage = transform.up;

        return bestAverage;
    }

    void DrawSafeVector()
    {
        Color debugRayColor = Color.magenta;
        Ray nextRay = new Ray(transform.position, targetSafeVector);
        
        Debug.DrawRay(transform.position, nextRay.direction * rayLength, debugRayColor);
    }
}
