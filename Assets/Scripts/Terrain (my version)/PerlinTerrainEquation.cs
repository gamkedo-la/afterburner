using UnityEngine;
using System.Collections.Generic;


public class PerlinTerrainEquation : TerrainHeightEquationBase
{
    public static Vector2 m_perlinOffset = Vector2.zero;

    [Header("Scale settings")]
    [SerializeField] float m_xScale = 0.01f;
    [SerializeField] float m_zScale = 0.01f;
    [SerializeField] float m_heightScale = 100f;
    [Range(1, 4)]
    [SerializeField] int m_power = 2;

    [Header("UV settings")]
    [SerializeField] Vector2 m_mountainPeakUv;
    [SerializeField] Vector2 m_mountainSlopeUv;
    [SerializeField] Vector2 m_footHillsUv;
    [SerializeField] Vector2 m_underwaterUv;

    [Header("Height UV settings")]
    [SerializeField] float m_mountainPeakLowestHeight = 100f;
    [SerializeField] float m_mountainSlopeLowestHeight = 50f;
    [SerializeField] float m_waterLine = 0f;


    public override float GetHeight(float x, float z)
    {
        float y = Mathf.PerlinNoise(x * m_xScale + m_perlinOffset.x, z * m_zScale + m_perlinOffset.y);

        //float height = 1f;

        //for (int i = 0; i < m_power; i++)
        //    height = height * y;

        float height = Mathf.Pow(y, m_power);

        return height * m_heightScale;
    }


    public override IList<Vector2> GetUv(IList<Vector3> triangle)
    {
        float maxHeight = float.NegativeInfinity;

        for (int i = 0; i < triangle.Count; i++)
        {
            float height = triangle[i].y;
            if (height > maxHeight)
                maxHeight = height;
        }

        Vector2 uvs = new Vector2();

        if (maxHeight < m_waterLine)
            uvs = m_underwaterUv;

        if (maxHeight >= m_mountainPeakLowestHeight)
            uvs = m_mountainPeakUv;

        if (maxHeight < m_mountainPeakLowestHeight
            && maxHeight >= m_mountainSlopeLowestHeight)
            uvs = m_mountainSlopeUv;

        var uvsList = new List<Vector2>();

        for (int i = 0; i < 3; i++)
            uvsList.Add(uvs);

        return uvsList;
    }


    public override IList<Color32> GetVertexColours(IList<Vector3> triangle)
    {
        var coloursList = new List<Color32>();

        for (int i = 0; i < 3; i++)
            coloursList.Add(Color.red);

        return coloursList;
    }


    public override void Randomise()
    {
        m_perlinOffset = 10000f * Random.onUnitSphere;
    }
}
