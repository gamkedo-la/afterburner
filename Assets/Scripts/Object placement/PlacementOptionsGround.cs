using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlacementOptionsGround : PlacementOptions
{
	public float minHeight = 0f;
	public float maxHeight = 1000f;
	public bool allignWithTerrain = true;
	public PlaceableObjectGround[] groundTypePrefabs;
}
