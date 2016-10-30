using UnityEngine;
using System.Collections;

public class TerrainEquationSineWave : TerrainEquationBase
{
    [SerializeField] float m_scale = 100f;


    public override float GetHeight(float x, float y)
    {
        float height = (Mathf.Sin(x / m_scale) + Mathf.Sin(y / m_scale) + 2f) * 0.25f;
        return height;
    }
}
