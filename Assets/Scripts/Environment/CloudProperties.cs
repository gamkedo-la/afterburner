using UnityEngine;
using System.Collections;

[System.Serializable]
public class CloudProperties
{
    public GameObject cloudPrefab;

    [Header("Distribution")]
    public int numberOfClouds = 30;
    [Space(10)]
    public float minAltitude = 50f;
    public float maxAltitude = 200f;
    [Space(10)]
    public float minSeparation = 200f;

    [Header("Appearance")]
    public float minScale = 3f;
    public float maxScale = 10f;
    
    [Space(10)]
    public Vector3 rotationAxis = Vector3.forward;	//Default for the low-poly pack
}
