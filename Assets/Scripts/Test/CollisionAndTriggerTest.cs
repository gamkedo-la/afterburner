using UnityEngine;
using System.Collections;

public class CollisionAndTriggerTest : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        print("Triggered by: " + other.name);
    }


    void OnCollisionEnter(Collision col)
    {
        print("Collided with: " + col.gameObject.name);
    }
}
