using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(TrailRenderer))]
public class BulletFlight : MonoBehaviour
{
	[SerializeField] int m_bulletDamage = 10;
    [SerializeField] float m_bulletLifeTime = 4f;
    [SerializeField] float m_bulletSpeed = 100f;
    [SerializeField] ParticleSystem m_explosion;
    [SerializeField] ParticleSystem m_splash;
    [SerializeField] float m_timeBeforeCanImpact = 0.1f;
    
    private Vector3 m_velocity;
    private bool m_bulletImpacted;
    private Transform m_originalParent;
    private float m_instantiationTime;


    void Start()
    {
        Destroy(gameObject, m_bulletLifeTime);
        m_originalParent = transform.root;
        transform.parent = null;
        SetInitialVelocity(Vector3.zero);
        m_instantiationTime = Time.time;
    }


	void Update()
    {
        transform.Translate(m_velocity * Time.deltaTime);
	}


    public void SetInitialVelocity(Vector3 velocity)
    {
        m_velocity = velocity + Vector3.forward * m_bulletSpeed;
    }


    void OnTriggerStay(Collider other)
    {
        if (m_bulletImpacted || (Time.time - m_instantiationTime < m_timeBeforeCanImpact))
            return;

        if ((other.transform == m_originalParent)
            || (other.transform.root == m_originalParent))
            return;

        //print("Bullet impact with " + other.name);

        ParticleSystem particles;

        if (other.CompareTag(Tags.Water))
            particles = m_splash;
        else
            particles = m_explosion;

        m_bulletImpacted = true;  

		IDamageable damageScript = other.gameObject.GetComponentInParent<IDamageable>();

		if (damageScript != null)
			damageScript.Damage(m_bulletDamage);

        var explosion = Instantiate(particles);
        explosion.transform.position = transform.position;

        var explosionAudio = explosion.gameObject.GetComponent<ExplosionAudioManager>();
        var audioClipBucket = other.gameObject.GetComponentInParent<AudioClipBucket>();

        float clipLength = explosion.startLifetime * 3f;

        if (explosionAudio != null && audioClipBucket != null)
            explosionAudio.SetClips(audioClipBucket.bulletHitAudioClips);

        if (explosionAudio != null)
            clipLength = explosionAudio.ClipLength;

        float lifetime = Mathf.Max(clipLength, explosion.startLifetime);

        Destroy(explosion.gameObject, lifetime);
        Destroy(gameObject);
    }
}
