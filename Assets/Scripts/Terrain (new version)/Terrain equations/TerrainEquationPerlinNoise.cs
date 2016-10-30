using UnityEngine;
using System.Collections;

public class TerrainEquationPerlinNoise : TerrainEquationBase
{
    [SerializeField] float m_scale = 100f;

    [SerializeField] int m_octaves = 5;
    [Range(0, 1)]
    [SerializeField] float m_persistance = 0.5f;
    [SerializeField] float m_lacunarity = 2f;

    //[SerializeField] Vector2 m_offset = Vector2.zero;   // Not currently used

    private Vector2[] m_octaveOffsets;
    private float m_totalAmplitude;


    public override void Initialise(int seed)
    {
        base.Initialise(seed);

        m_totalAmplitude = 0;
        float amplitude = 1;

        System.Random prng = new System.Random(seed);
        m_octaveOffsets = new Vector2[m_octaves];
        for (int i = 0; i < m_octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            m_octaveOffsets[i] = new Vector2(offsetX, offsetY);
            m_totalAmplitude += amplitude;
            amplitude *= m_persistance;
        }

        if (m_scale <= 0)
            m_scale = 0.0001f;
    }


    public override float GetHeight(float x, float y)
    {
        //return 0.5f;
        float amplitude = 1;
        float frequency = 1;
        float height = 0;
        
        for (int i = 0; i < m_octaves; i++)
        {
            //float sampleX = x / m_scale * frequency + m_octaveOffsets[i].x;
            //float sampleY = y / m_scale * frequency + m_octaveOffsets[i].y;
            float sampleX = (x + m_octaveOffsets[i].x) / m_scale * frequency;
            float sampleY = (y + m_octaveOffsets[i].y) / m_scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            height += perlinValue * amplitude;     

            amplitude *= m_persistance;
            frequency *= m_lacunarity;
        }

        height /= m_totalAmplitude;

        return height;
    }


    void OnValidate()
    {
        if (m_lacunarity < 1)
        {
            m_lacunarity = 1;
        }
        if (m_octaves < 0)
        {
            m_octaves = 0;
        }
    }
}
