using UnityEngine;
using System.Collections;

public class DestroyObject : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Ground) || other.CompareTag(Tags.Water))
            return;

        var placeableObject = other.gameObject.GetComponentInParent<PlaceableObject>();

        if (placeableObject != null)
            Destroy(placeableObject.gameObject);
    }
}
