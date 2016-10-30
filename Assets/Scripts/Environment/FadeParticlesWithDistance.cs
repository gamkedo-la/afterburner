using UnityEngine;
using System.Collections;

public class FadeParticlesWithDistance : MonoBehaviour
{
    private ParticleSystem[] m_emittersToFade;
    private ParticleSystem.MinMaxGradient colorGrad;
    private CloudGenerator m_cloudGenerator;
    private Transform m_camera;


    void Start()
    {
        m_cloudGenerator = FindObjectOfType<CloudGenerator>();
        m_emittersToFade = GetComponentsInChildren<ParticleSystem>();
        m_camera = Camera.main.transform;
    }


    void Update()
    {
        Color fadedCol = Color.white;
        float camDist = Vector3.Distance(transform.position, m_camera.position);

        fadedCol.a = 1.0f - (camDist / m_cloudGenerator.MaxDistance);
        // fadedCol.a *= fadedCol.a; // can square % for diff. distance falloff
        colorGrad = new ParticleSystem.MinMaxGradient(fadedCol);

        for (int i = 0; i < m_emittersToFade.Length; i++)
        {
            // emittersToFade[i].startColor = fadedCol; // only affects new emits
            var overLifetime = m_emittersToFade[i].colorOverLifetime;
            overLifetime.color = colorGrad;
        }
    }
}
