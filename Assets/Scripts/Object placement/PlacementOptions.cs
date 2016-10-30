using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlacementOptions
{
    public int number = 10;
    public float minSeparation = 50f;
    public float minDistFromMainTarget = 100f;
    public float maxDistFromMainTarget = 1000f;
    public float minAngleFromNorth = -180f;
    public float maxAngleFromNorth = 180f;
}
