using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour, IDamageable 
{
	[SerializeField] int m_startingHealth = 100;
    [SerializeField] int m_damageCausedToOthers = 100;

    [SerializeField] GameObject m_aliveModel;
    [SerializeField] GameObject m_deadModel;
	[SerializeField] ParticleSystem m_explosion; 
    [SerializeField] ParticleSystem m_waterSplash;
    [SerializeField] GameObject m_fireAndSmoke;
    [SerializeField] Transform[] m_fireSpawnPoints;
    [SerializeField] ExplosionTrigger m_impactExplosionTrigger;

    [SerializeField] float m_fireInFlightRateMultiplier = 5f;
    [SerializeField] float m_fireInFlightLifetimeMultiplier = 0.2f;
    [SerializeField] float m_smokeInFlightRateMultiplier = 2f;
    [SerializeField] float m_smokeInFlightLifetimeMultiplier = 0.5f;
    
    [SerializeField] bool m_becomePhysicsObjectOnDeath;
    [SerializeField] bool m_alignWithFlightDirection = true;
    [SerializeField] bool m_allowDestroyedByGround;
    [SerializeField] bool m_allowDestroyedByWater;
    [SerializeField] bool m_explodeOnCrashAfterDeath = true;
    //[SerializeField] bool m_killChildrenOnDeath;

    [SerializeField] float m_rigidbodyDragInWater = 3f;
    [SerializeField] float m_rigidbodyAngularDragInWater = 1f;
    [SerializeField] Transform[] m_objectsToDetatchOnDeath;
    [SerializeField] Transform[] m_objectsToTransferToDeadModelOnDeath;
    [SerializeField] float m_canDamageResetTime = 0.15f;
    [SerializeField] int m_minDamageForReset = 100;
    [SerializeField] float m_maxSpinRateOnDeath = 30f;

    [SerializeField] bool m_allowKillKey = false;

	private int m_currentHealth;
    private bool m_dead;
    private Vector3 m_impactPoint;
    private AudioClipBucket m_audioClipBucket;
    private Rigidbody m_rigidBody;
    private bool m_inWater;
    private bool m_crashedOnGround;
    private List<GameObject> m_activeFireAndSmoke;
    private float m_originalFireParticleLifetime;
    private float m_originalFireParticleEmissionRate;
    private float m_originalSmokeParticleLifetime;
    private float m_originalSmokeParticleEmissionRate;
    private float m_spinRate;
    private bool m_canTakeDamage = true;


    void Awake()
	{
        m_impactPoint = transform.position;
		m_currentHealth = m_startingHealth;
        m_audioClipBucket = GetComponent<AudioClipBucket>();
        m_rigidBody = GetComponent<Rigidbody>();

        if (m_aliveModel != null && m_deadModel != null)
        {
            m_aliveModel.SetActive(true);
            m_deadModel.SetActive(false);
        }

        if (m_fireSpawnPoints.Length == 0)
        {
            m_fireSpawnPoints = new Transform[1];
            m_fireSpawnPoints[0] = transform;
        }
    }


    void Update()
    {
        if (m_allowKillKey && Input.GetKeyDown(KeyCode.Q))
        {
            m_impactPoint = transform.position;
            Damage(m_startingHealth);
        }
    }


    void FixedUpdate()
    {
        if (!m_becomePhysicsObjectOnDeath || !m_dead || m_rigidBody == null 
            || m_rigidBody.isKinematic || m_inWater || m_crashedOnGround ||!m_alignWithFlightDirection)
            return;

        float rotationZ = m_rigidBody.rotation.eulerAngles.z;
        var rotation = Quaternion.LookRotation(m_rigidBody.velocity);
        var eulerAngles = rotation.eulerAngles;
        eulerAngles.z = rotationZ += m_spinRate * Time.deltaTime;
        rotation = Quaternion.Euler(eulerAngles);

        m_rigidBody.MoveRotation(rotation);
    }


    public void Damage(int damage)
    {
        if (m_dead || !m_canTakeDamage)
            return;

        m_currentHealth -= damage;

        //print(string.Format("{0} damaged by {1}, current health = {2} (time: {3})", name, damage, m_currentHealth, Time.time));

        if (damage >= m_minDamageForReset)
            DisableBriefly();

        if (m_currentHealth <= 0)
            Dead();
    }


    public void DisableBriefly()
    {
        //print(string.Format("{0} disabled for {1}s", name, m_canDamageResetTime));
        m_canTakeDamage = false;
        StartCoroutine(ResetTransformJustDamaged());
    }


    void OnTriggerEnter(Collider other)
    {
        m_impactPoint = other.transform.position;

        if (other.CompareTag(Tags.Bullet)
            // To make sure no collisions happen during the placement algorithm
            || Time.time < ProceduralPlacement.TimePlacementFinished
            || Time.timeSinceLevelLoad < 0.15)
            return;

        if (other.CompareTag(Tags.Water) && !m_inWater)
        {
            if (m_waterSplash != null && m_allowDestroyedByWater)
                InstantiateWaterSplash(transform.position);

            EnterWater();
        }

        if (other.CompareTag(Tags.Water) && !m_allowDestroyedByWater)
            return;

        if (other.CompareTag(Tags.Ground) && !m_allowDestroyedByGround)
            return;

        var otherDamageScript = other.gameObject.GetComponentInParent<IDamageable>();

        if (otherDamageScript != null && !other.CompareTag(Tags.Bullet))
        {
            //print(string.Format("{0} causes {1} damage to {2}", name, m_damageCausedToOthers, other.name));
            otherDamageScript.Damage(m_damageCausedToOthers);
        }
        else
            Dead();
    }


    void OnCollisionEnter(Collision col)
    {
        if (!m_becomePhysicsObjectOnDeath || m_inWater || m_crashedOnGround 
            || (col.gameObject.CompareTag(Tags.Ground) && !m_allowDestroyedByGround))
            return;      

        if (col.gameObject.CompareTag(Tags.Ground) && m_explosion != null)
        {
            //print(name + " crashed into ground: " + col.contacts[0].point);
            m_crashedOnGround = true;

            if (m_explodeOnCrashAfterDeath)
                InstantiateExplosion(col.contacts[0].point);

            m_rigidBody.velocity *= 0.1f;
            m_rigidBody.drag = 1f;

            for (int j = 0; j < m_activeFireAndSmoke.Count; j++)
            {
                var particleSystems = m_activeFireAndSmoke[j].GetComponentsInChildren<ParticleSystem>();

                for (int i = 0; i < particleSystems.Length; i++)
                {
                    var particleSystem = particleSystems[i];

                    if (particleSystem.name == "Fire")
                    {
                        particleSystem.startLifetime = m_originalFireParticleLifetime;
                        var emission = particleSystem.emission;
                        var rate = emission.rate;
                        rate.constantMax = m_originalFireParticleEmissionRate;
                        rate.constantMin = m_originalFireParticleEmissionRate;
                        emission.rate = rate;
                    }
                    else if (particleSystem.name == "Smoke")
                    {
                        particleSystem.startLifetime = m_originalSmokeParticleLifetime;
                        var emission = particleSystem.emission;
                        var rate = emission.rate;
                        rate.constantMax = m_originalSmokeParticleEmissionRate;
                        rate.constantMin = m_originalSmokeParticleEmissionRate;
                        emission.rate = rate;
                    }
                }
            }
        }
    }


    private void EnterWater()
    {
        m_inWater = true;
        m_rigidBody.drag = m_rigidbodyDragInWater;
        m_rigidBody.angularDrag = m_rigidbodyAngularDragInWater;
    }


    private IEnumerator ResetTransformJustDamaged()
    {
        yield return new WaitForSeconds(m_canDamageResetTime);

        m_canTakeDamage = true;
    }


    private void InstantiateWaterSplash(Vector3 position)
    {
        var waterSplash = Instantiate(m_waterSplash);
        waterSplash.transform.position = position;

        float lifetime = waterSplash.startLifetime; // Mathf.Max(clipLength, explosion.startLifetime);
        Destroy(waterSplash.gameObject, lifetime * 5f);
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


    private void InitialiseFireAndSmokeParameters(GameObject fireAndSmoke)
    {
        if (m_allowDestroyedByGround)
        {
            var particleSystems = fireAndSmoke.GetComponentsInChildren<ParticleSystem>();

            for (int i = 0; i < particleSystems.Length; i++)
            {
                var particleSystem = particleSystems[i];

                if (particleSystem.name == "Fire")
                {
                    m_originalFireParticleLifetime = particleSystem.startLifetime;
                    particleSystem.startLifetime = m_originalFireParticleLifetime * m_fireInFlightLifetimeMultiplier;
                    var emission = particleSystem.emission;
                    var rate = emission.rate;
                    m_originalFireParticleEmissionRate = rate.constantMax;
                    rate.constantMax = m_originalFireParticleEmissionRate * m_fireInFlightRateMultiplier;
                    rate.constantMin = m_originalFireParticleEmissionRate * m_fireInFlightRateMultiplier;
                    emission.rate = rate;
                }
                else if (particleSystem.name == "Smoke")
                {
                    m_originalSmokeParticleLifetime = particleSystem.startLifetime;
                    particleSystem.startLifetime = m_originalSmokeParticleLifetime * m_smokeInFlightLifetimeMultiplier;
                    var emission = particleSystem.emission;
                    var rate = emission.rate;
                    m_originalSmokeParticleEmissionRate = rate.constantMax;
                    rate.constantMax = m_originalSmokeParticleEmissionRate * m_smokeInFlightRateMultiplier;
                    rate.constantMin = m_originalSmokeParticleEmissionRate * m_smokeInFlightRateMultiplier;
                    emission.rate = rate;
                }
            }
        }
    }


    private void InstantiateFireAndSmoke(Vector3 position)
    {
        var fireAndSmoke = (GameObject) Instantiate(m_fireAndSmoke, position, m_fireAndSmoke.transform.rotation);
        fireAndSmoke.transform.parent = transform;

        AddFireAndSmoke(fireAndSmoke);
    }


    private void TriggerExplosionChainReaction()
    {
        m_activeFireAndSmoke = new List<GameObject>();

        if (m_impactExplosionTrigger != null)
        {
            m_impactExplosionTrigger.transform.position = m_impactPoint;
            m_impactExplosionTrigger.Explode();
        }
        else
        {
            InstantiateExplosion(transform.position);
            InstantiateFireAndSmoke(transform.position);
        }
    }


    private void Dead()
    {
        if (m_dead)
            return;

        m_dead = true;

		for(int i = 0; i < m_objectsToDetatchOnDeath.Length; i++) {
			m_objectsToDetatchOnDeath[i].parent = null;
		}

        EventManager.TriggerEvent(TransformEventName.EnemyDead, transform);

        if (m_aliveModel != null && m_deadModel != null)
        {
            m_aliveModel.SetActive(false);
            m_deadModel.SetActive(true);
//Debug.Log("ded");
            for (int i = 0; i < m_objectsToTransferToDeadModelOnDeath.Length; i++)
            {
                var objectToTransfer = m_objectsToTransferToDeadModelOnDeath[i];

                if (objectToTransfer != null)
                {
                    objectToTransfer.parent = m_deadModel.transform;

                    var healthScript = objectToTransfer.GetComponent<EnemyHealth>();

                    if (healthScript != null)
                        healthScript.DisableBriefly();
                }
            }

            TriggerExplosionChainReaction();
        }
        else if (!m_becomePhysicsObjectOnDeath)
            Destroy(gameObject, 0.05f);
        
        if (m_becomePhysicsObjectOnDeath && m_rigidBody != null)
            BecomePhysicsObject();

        //if (m_killChildrenOnDeath)
        //{
        //    var childHealthScripts = GetComponentsInChildren<EnemyHealth>();

        //    for (int i = 0; i < childHealthScripts.Length; i++)
        //        childHealthScripts[i].Dead();
        //}

        if (m_currentHealth > 0)
            m_currentHealth = 0;
    }


    private void BecomePhysicsObject()
    {
        m_rigidBody.isKinematic = false;
        m_rigidBody.useGravity = true;

        var flightControlScript = GetComponent<FlyingControl>();
        var flightAiInputScript = GetComponent<EnemyAircraftAiInput>();
        var shootingControlScript = GetComponent<ShootingControl>();
        var shootingAiInputScript = GetComponent<EnemyShootingAiInput>();

        if (flightControlScript != null)
        {
            m_rigidBody.velocity = flightControlScript.ForwardVelocityInWorld;
            flightControlScript.enabled = false;
        }

        if (flightAiInputScript != null)
            flightAiInputScript.enabled = false;

        if (shootingControlScript != null)
            shootingControlScript.enabled = false;

        if (shootingAiInputScript != null)
            shootingAiInputScript.enabled = false;

        m_spinRate = Random.Range(-m_maxSpinRateOnDeath, m_maxSpinRateOnDeath);
        //print("Spin rate: " + m_spinRate);

        StartCoroutine(WaitForSleep());
    }


    private IEnumerator WaitForSleep()
    {
        // Wait for the first time the ridigbody comes to a stop, i.e. the initial crash
        while (!m_rigidBody.IsSleeping() && m_rigidBody.velocity.sqrMagnitude > 0.1f)
            yield return null;

        //print(Time.time + ": Rigidbody stopped for first time");

        // Then wait to see if it starts moving again
        yield return new WaitForSeconds(1f);

        //print(Time.time + ": Checking for any further movement");

        // Finally, wait for the second time the ridigbody comes to a stop, i.e. settled
        while (!m_rigidBody.IsSleeping() && m_rigidBody.velocity.sqrMagnitude > 0.1f)
            yield return null;

        //print(Time.time + ": Rigidbody stopped for second time");

        m_rigidBody.isKinematic = true;
        m_rigidBody.useGravity = false;
    }


    public void AddFireAndSmoke(GameObject fireAndSmoke)
    {
        if (m_activeFireAndSmoke == null)
            m_activeFireAndSmoke = new List<GameObject>();

        m_activeFireAndSmoke.Add(fireAndSmoke);
        InitialiseFireAndSmokeParameters(fireAndSmoke);
    }


    public bool IsDead
    {
        get { return m_dead; }
    }


    public int CurrentHealth
    {
        get { return m_currentHealth; }
    }


    public int StartingHealth
    {
        get { return m_startingHealth; }
    }


    public bool BecomesPhysicsObjectOnDeath
    {
        get { return m_becomePhysicsObjectOnDeath; }
    }


    public bool IsCrashedOnGround
    {
        get { return m_crashedOnGround; }
    }


    public bool IsInWater
    {
        get { return m_inWater; }
    }
}
