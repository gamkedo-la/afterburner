using UnityEngine;
using System.Collections;

public class TerrainEquationFlat : TerrainEquationBase
{
    [SerializeField] float m_height = 2f;

    public override float GetHeight(float x, float y)
    {
        return m_height;
    }
}
