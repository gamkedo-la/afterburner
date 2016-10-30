using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlacementOptionsWater : PlacementOptions
{
    public bool alignWithMainTarget = true;

    public PlaceableObjectWater[] waterDefenceTypePrefabs;
}
