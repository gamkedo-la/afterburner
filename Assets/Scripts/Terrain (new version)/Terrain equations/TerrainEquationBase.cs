using UnityEngine;
using System.Collections;

public abstract class TerrainEquationBase : MonoBehaviour, ITerrainEquation
{
    public int seed = 0;


    public virtual void Initialise(int seed)
    {
        this.seed = seed;
    }


    public virtual float GetHeight(float x, float y)
    {
        return 0f;
    }
}
