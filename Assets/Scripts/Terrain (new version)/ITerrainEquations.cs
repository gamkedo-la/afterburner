using UnityEngine;
using System.Collections;

public interface ITerrainEquations
{
    /// <summary>
    /// Get the noise value for a given x-y pixel corrdinate 
    /// </summary>
    /// <param name="x">The x-coordinate in pixels</param>
    /// <param name="y">The y-coordinate in pixels</param>
    /// <returns>The noise value between 0 and 1</returns>
    float GetNoise(int x, int y);
}
