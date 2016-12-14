using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlacementOptionsFoliage : PlacementOptions
{
	public float minHeight = 0f;
	public float maxHeight = 1000f;
	public bool orientWithTerrain = false;
	public float minScale = 1f;
	public float maxScale = 1f;
	public float maxYStretch = 1f;
	public PlaceableObjectFoliage[] foliageTypePrefabs;
}
