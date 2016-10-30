using UnityEngine;
using System.Collections;

public class FireManager : MonoBehaviour
{
    [SerializeField] float m_fireLifeTimeSeconds = 120f;
    [SerializeField] bool m_ignoreWaterTrigger;

    private Light m_light;
    private ParticleSystem[] m_particleSystems;
    private IDamageable m_healthScript;

    private WaitForSeconds m_wait;
    private float[] emissionRates;

    void Awake()
    {
        m_light = GetComponentInChildren<Light>();
        m_particleSystems = GetComponentsInChildren<ParticleSystem>();
        m_wait = new WaitForSeconds(1f);  
    }


    void Start()
    {
        // Need to do this in Start rather than Awake otherwise it's always null
        m_healthScript = GetComponentInParent<IDamageable>();

        if (m_healthScript != null)
            StartCoroutine(CheckForCrashedOnGround());
    }


    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.Water)) 
            return;

        for (int i = 0; i < m_particleSystems.Length; i++)
            m_particleSystems[i].Stop();

        if (m_light != null)
            m_light.gameObject.SetActive(false);
    }


    private IEnumerator CheckForCrashedOnGround()
    {
        bool onGround = false;

        while(!onGround)
        {
            onGround = m_healthScript.IsCrashedOnGround 
                || m_healthScript.IsInWater
                || !m_healthScript.BecomesPhysicsObjectOnDeath;

            yield return m_wait;
        }

        StartCoroutine(BurnDownFire());
    }


    private IEnumerator BurnDownFire()
    {
        float startLightRange = m_light.range;
        var startLifetimes = new float[m_particleSystems.Length]; 
        var startEmissionRates = new float[m_particleSystems.Length];

        for (int i = 0; i < m_particleSystems.Length; i++)
        {
            var particleSystem = m_particleSystems[i];
            var emission = particleSystem.emission;
            var rate = emission.rate;
            startLifetimes[i] = particleSystem.startLifetime;
            startEmissionRates[i] = rate.constantMax;
        }

        float timeAtBurnDownStart = Time.time;
        float fraction = 0;

        while (fraction < 1f)
        {
            yield return m_wait;

            fraction = (Time.time - timeAtBurnDownStart) / m_fireLifeTimeSeconds;
            float fractionLeft = 1f - fraction;

            for (int i = 0; i < m_particleSystems.Length; i++)
            {
                var particleSystem = m_particleSystems[i];
                float newEmissionRate = startEmissionRates[i] * fractionLeft;
                float newLifetime = startLifetimes[i] * fractionLeft;
                var emission = particleSystem.emission;
                var rate = emission.rate;
                rate.constantMin = newEmissionRate;
                rate.constantMax = newEmissionRate;
                emission.rate = rate;
                particleSystem.startLifetime = newLifetime;
                m_light.range = startLightRange * fractionLeft;
            }
        }

        for (int i = 0; i < m_particleSystems.Length; i++)
            m_particleSystems[i].Stop();

        m_light.gameObject.SetActive(false);
    }
}
