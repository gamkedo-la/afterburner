using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class TerrainHeightEquationBase : MonoBehaviour, ITerrainHeightEquation
{
    public virtual float GetHeight(float x, float z)
    {
        return 0f;
    }


    public virtual IList<Vector2> GetUv(IList<Vector3> triangle)
    {
        var uvs = new Vector2(0.5f, 0.5f);
        var uvsList = new List<Vector2>();

        for (int i = 0; i < 3; i++)
            uvsList.Add(uvs);

        return  uvsList;
    }


    public virtual IList<Color32> GetVertexColours(IList<Vector3> triangle)
    {
        var coloursList = new List<Color32>();

        for (int i = 0; i < 3; i++)
            coloursList.Add(Color.white);

        return coloursList;
    }


    public abstract void Randomise();
}
