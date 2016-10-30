using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public interface ITerrainHeightEquation
{
    /// <summary>
    /// Get the height for a given x-z corrdinate 
    /// </summary>
    /// <param name="x">The x-coordinate in world space</param>
    /// <param name="z">The z-coordinate in world space</param>
    /// <returns>The y-coordinate in world space</returns>
    float GetHeight(float x, float z);


    /// <summary>
    /// Get the UV coordinate to use for all the vertices in a triangle
    /// </summary>
    /// <param name="triangle">The set of vertices making up a single triangle</param>
    /// <returns>The UV coordinate for all vertices</returns>
    IList<Vector2> GetUv(IList<Vector3> triangle);


    /// <summary>
    /// Get the vertex colours for all the vertices in a triangle
    /// </summary>
    /// <param name="triangle">The set of vertices making up a single triangle</param>
    /// <returns>The colours for all the vertices</returns>
    IList<Color32> GetVertexColours(IList<Vector3> triangle);
}
