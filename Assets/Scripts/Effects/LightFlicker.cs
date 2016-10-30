using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{
    [SerializeField] Vector2 m_intensityMultiplierMinMax = new Vector2(0.8f, 1.2f);
    [SerializeField] float m_flickerFrequency = 4f;
    [SerializeField] float m_lightFollowSpeed = 2f;

    private Light m_light;
    private float m_intensity;
    private float m_perlinX;
    private Transform m_parent;
    private Vector3 m_parentOffset;

    
    void Awake()
    {
        m_light = GetComponent<Light>();
        m_intensity = m_light.intensity;
        m_perlinX = Random.Range(-100f, 100f);
        m_parent = transform.parent;
        m_parentOffset = transform.position - m_parent.position;      
    }
	

    void Start()
    {
        transform.parent = null;
    }


	void Update()
    {
        float noise = Mathf.PerlinNoise(m_perlinX, Time.time * m_flickerFrequency);
        m_light.intensity = m_intensity * Mathf.Lerp(m_intensityMultiplierMinMax.x, m_intensityMultiplierMinMax.y, noise);

        if (m_parent!= null)
            transform.position = Vector3.Lerp(transform.position, m_parent.position + m_parentOffset, Time.deltaTime * m_lightFollowSpeed);
	}
}
