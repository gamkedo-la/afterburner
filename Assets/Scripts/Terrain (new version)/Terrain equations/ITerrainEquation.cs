using UnityEngine;
using System.Collections;

public interface ITerrainEquation
{
    // Perform any initialisation that might be required
    void Initialise(int seed);


    /// <summary>
    /// Get the height value for a given x-y corrdinate 
    /// </summary>
    /// <param name="x">The x-coordinate</param>
    /// <param name="y">The y-coordinate</param>
    /// <returns>The height value</returns>
    float GetHeight(float x, float y);
}
