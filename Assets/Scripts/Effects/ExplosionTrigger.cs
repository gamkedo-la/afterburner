using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] ParticleSystem m_explosion;
    [SerializeField] GameObject m_fireAndSmoke;
    [SerializeField] float m_explosionChainReactionMaxDelay = 0.5f;
    [SerializeField] float m_explosionChainReactionRadius = 60f;
    [SerializeField] int m_damageInflicted = 100;

    private EnemyHealth m_enemyHealthScript;
    private AudioClipBucket m_audioClipBucket;
    private GameObject m_activeFireAndSmoke;
    private bool m_triggered;


    void Awake()
    {
        m_enemyHealthScript = GetComponentInParent<EnemyHealth>();
        m_audioClipBucket = GetComponentInParent<AudioClipBucket>();
    }


    public void Explode()
    {
        if (m_triggered)
            return;

        m_triggered = true;

        InstantiateExplosion(transform.position);
        InstantiateFireAndSmoke(transform.position);

        if (m_enemyHealthScript != null)
            m_enemyHealthScript.AddFireAndSmoke(m_activeFireAndSmoke);

        var colliders = Physics.OverlapSphere(transform.position, m_explosionChainReactionRadius);

        for (int i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            var explosionTrigger = collider.GetComponent<ExplosionTrigger>();
            var healthScript = collider.GetComponentInParent<EnemyHealth>();

            if (explosionTrigger != null)
            {
                float distance = (transform.position - collider.transform.position).magnitude;
                float delay = m_explosionChainReactionMaxDelay * distance / m_explosionChainReactionRadius;

                StartCoroutine(TriggerNearbyExplosions(explosionTrigger, delay));
            }

            if (healthScript != null)
            {
                float distance = (transform.position - collider.transform.position).magnitude;
                float delay = m_explosionChainReactionMaxDelay * distance / m_explosionChainReactionRadius;

                StartCoroutine(DamageNearbyObjects(healthScript, delay));
            }
        }       
    }


    private IEnumerator TriggerNearbyExplosions(ExplosionTrigger explosionTrigger, float delay)
    {
        yield return new WaitForSeconds(delay);

        explosionTrigger.Explode();
    }


    private IEnumerator DamageNearbyObjects(IDamageable healthScript, float delay)
    {
        yield return new WaitForSeconds(delay);

        healthScript.Damage(m_damageInflicted);
    }


    private void InstantiateExplosion(Vector3 position)
    {
        var explosion = Instantiate(m_explosion);
        explosion.transform.position = position;

        var explosionAudio = explosion.gameObject.GetComponent<ExplosionAudioManager>();
        float clipLength = 0;

        if (explosionAudio != null)
        {
            clipLength = explosionAudio.ClipLength;

            if (m_audioClipBucket != null)
                explosionAudio.SetClips(m_audioClipBucket.explosionAudioClips);
        }

        float lifetime = Mathf.Max(clipLength, explosion.startLifetime);
        Destroy(explosion.gameObject, lifetime * 1.5f);
    }


    private void InstantiateFireAndSmoke(Vector3 position)
    {
        m_activeFireAndSmoke = (GameObject) Instantiate(m_fireAndSmoke, position, m_fireAndSmoke.transform.rotation);
        m_activeFireAndSmoke.transform.parent = transform;
    }
}
