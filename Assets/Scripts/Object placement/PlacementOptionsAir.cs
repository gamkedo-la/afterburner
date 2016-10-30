using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlacementOptionsAir : PlacementOptions
{
    public float minAltitude = 100f;
    public float maxAltitude = 1000f;

    public PlaceableObjectAir[] aircraftTypePrefabs;
}
