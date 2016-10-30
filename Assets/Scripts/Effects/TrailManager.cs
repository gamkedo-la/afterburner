using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class TrailManager : MonoBehaviour
{
    [SerializeField] float m_fadeLifetimeFraction = 0.5f;

    private TrailRenderer m_trail;
    private bool m_fading;
    private float m_startWidth;
    private float m_endWidth;


    void Awake()
    {
        m_trail = GetComponent<TrailRenderer>();

        m_startWidth = m_trail.startWidth;
        m_endWidth = m_trail.endWidth;
    }
	

	void Update()
    {
        if (m_trail.transform.parent == null && !m_fading)
            StartCoroutine(FadeTrail());
	}


    private IEnumerator FadeTrail()
    {
        m_fading = true;
        float timeFadeStart = Time.time;

        while (true)
        {
            float timePassed = Time.time - timeFadeStart;
            float fractionOfLifetime = timePassed / (m_trail.time * m_fadeLifetimeFraction);
            float multiplier = Mathf.Lerp(1f, 0f, fractionOfLifetime);

            m_trail.startWidth = m_startWidth * multiplier;
            m_trail.endWidth = m_endWidth * multiplier;

            yield return null;
        }
    }
}
