using UnityEngine;
using System.Collections;

public class FadeRenderersWithDistance : MonoBehaviour
{
    [SerializeField] float m_distanceOfMinAlpha = 1800f;
    [SerializeField] float m_disatnceOfMaxAlpha = 1400f;
    [Space(10)]
    [Range(0f, 1f)]
    [SerializeField] float m_minAlpha = 0f;
    [Range(0f, 1f)]
    [SerializeField] float m_maxAlpha = 1f;

    private float m_distanceOfMaxAlphaSq;
    private float m_distanceOfMinAlphaSq;
    private Vector2 m_direction;
    private Color m_cloudColour;
    private Transform m_mainCamera;
    private MeshRenderer[] m_renderers;


    void Start()
    {
        m_mainCamera = Camera.main.transform;
        m_renderers = GetComponentsInChildren<MeshRenderer>();
        m_distanceOfMaxAlphaSq = m_disatnceOfMaxAlpha * m_disatnceOfMaxAlpha;
        m_distanceOfMinAlphaSq = m_distanceOfMinAlpha * m_distanceOfMinAlpha;   
    }
	

	void Update ()
    {
        var direction3 = m_mainCamera.position - transform.position;

        m_direction.Set(direction3.x, direction3.z);

        float distanceSq = m_direction.sqrMagnitude;

        float alphaFraction = distanceSq < m_distanceOfMaxAlphaSq
                ? 1f
                : 1f - ((distanceSq - m_distanceOfMaxAlphaSq) / (m_distanceOfMinAlphaSq - m_distanceOfMaxAlphaSq));

        float alpha = alphaFraction * (m_maxAlpha - m_minAlpha) + m_minAlpha;
        alpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < m_renderers.Length; i++)
        {
            var renderer = m_renderers[i];

            if (alpha > 0.999)
                renderer.material.SetInt("_Mode", 0);
            else
                renderer.material.SetInt("_Mode", 2);

            m_cloudColour = renderer.material.color;
            m_cloudColour.a = alpha;
            renderer.material.color = m_cloudColour;
        }
    }
}
