using UnityEngine;
using System.Collections;

public struct BoundsData
{
    public BoundsData(Vector3 corner1, Vector3 corner2, Vector3 corner3, Vector3 corner4, Vector3 centre,
        float minTerrainHeight, float maxTerrainHeight, float originAboveBase,
        float terrainHeightCorner1, float terrainHeightCorner2, float terrainHeightCorner3, 
        float terrainHeightCorner4, float terrainHeightCentre)
    {
        this.corner1 = corner1;
        this.corner2 = corner2;
        this.corner3 = corner3;
        this.corner4 = corner4;
        this.centre = centre;

        this.minTerrainHeight = minTerrainHeight;
        this.maxTerrainHeight = maxTerrainHeight;
        this.originAboveBase = originAboveBase;

        this.terrainHeightCorner1 = terrainHeightCorner1;
        this.terrainHeightCorner2 = terrainHeightCorner2;
        this.terrainHeightCorner3 = terrainHeightCorner3;
        this.terrainHeightCorner4 = terrainHeightCorner4;
        this.terrainHeightCentre = terrainHeightCentre;
    }


    public Vector3 corner1;
    public Vector3 corner2;
    public Vector3 corner3;
    public Vector3 corner4;
    public Vector3 centre;

    public float minTerrainHeight;
    public float maxTerrainHeight;
    public float originAboveBase;

    public float terrainHeightCorner1;
    public float terrainHeightCorner2;
    public float terrainHeightCorner3;
    public float terrainHeightCorner4;
    public float terrainHeightCentre;


    public float HeightDifference()
    {
        return maxTerrainHeight - minTerrainHeight;
    }
}
