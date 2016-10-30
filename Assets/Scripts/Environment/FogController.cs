using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    [SerializeField] bool m_legacyFogEnabled = false;
    [SerializeField] Color m_fogColour = Color.grey;
    [SerializeField] FogMode m_fogMode = FogMode.Exponential;
    [SerializeField] float m_exponentialFogDensity = 0.01f;
    [SerializeField] float m_linearFogStartDistance = 0f;
    [SerializeField] float m_linearFogEndDistance = 1000f;


    void Awake()
    {

    }


	void Update()
    {
        RenderSettings.fog = m_legacyFogEnabled;
        RenderSettings.fogColor = m_fogColour;
        RenderSettings.fogMode = m_fogMode;
        RenderSettings.fogDensity = m_exponentialFogDensity;
        RenderSettings.fogStartDistance = m_linearFogStartDistance;
        RenderSettings.fogEndDistance = m_linearFogEndDistance;
	}
}
