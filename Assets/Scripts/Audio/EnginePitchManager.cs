using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnginePitchManager : MonoBehaviour
{
    [SerializeField] Vector2 m_pitchMinMax = new Vector2(0.7f, 1.2f);

    private AudioSource m_audio;
    private FlyingControl m_flyingScript;
    private float m_maxSpeed;


    void Awake()
    {
        m_audio = GetComponent<AudioSource>();
        m_flyingScript = GetComponentInParent<FlyingControl>();

        if (m_flyingScript != null)
            m_maxSpeed = m_flyingScript.MaxForwardSpeed;
    }


    void Update()
    {
        if (m_flyingScript == null)
            return;

        float t = m_flyingScript.ForwardSpeed / m_maxSpeed;

        float pitch = Mathf.Lerp(m_pitchMinMax.x, m_pitchMinMax.y, t);

        m_audio.pitch = pitch;
    }
}
