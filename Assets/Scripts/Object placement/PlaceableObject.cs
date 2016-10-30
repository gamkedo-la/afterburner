using UnityEngine;
using System.Collections;

public class PlaceableObject : MonoBehaviour
{
    public Bounds? GetUnrotatedBounds()
    {
        var rotation = transform.rotation;
        transform.rotation = Quaternion.identity;

        var bounds = BoundsUtilities.OverallBounds(gameObject);

        transform.rotation = rotation;

        return bounds;
    }
}
