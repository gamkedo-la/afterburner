using UnityEngine;
using System.Collections;


public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static bool PlayerDead;

    [SerializeField] int m_startingHealth = 300;
    [SerializeField] int m_damageCausedToOthers = 100;
    [SerializeField] bool m_invulnerable = false;

    [SerializeField] GameObject m_aliveModel;
    [SerializeField] GameObject m_deadModel;
    [SerializeField] ParticleSystem m_explosion;
    [SerializeField] ParticleSystem m_waterSplash;
    [SerializeField] GameObject m_fireAndSmoke;
    [SerializeField] Transform[] m_fireSpawnPoints;

    [SerializeField] float m_fireInFlightRateMultiplier = 5f;
    [SerializeField] float m_fireInFlightLifetimeMultiplier = 0.2f;
    [SerializeField] float m_smokeInFlightRateMultiplier = 2f;
    [SerializeField] float m_smokeInFlightLifetimeMultiplier = 0.5f;
    
    [SerializeField] float m_rigidBodyDragInWater = 3f;
    [SerializeField] Transform[] m_objectsToDetatchOnDeath;
    [SerializeField] float m_canDamageResetTime = 0.15f;
    [SerializeField] int m_minDamageForReset = 100;
    [SerializeField] float m_maxSpinRateOnDeath = 30f;  

    [SerializeField] float m_cameraShakeMagnitude = 1f;
    [SerializeField] float m_cameraShakeDuration = 0.1f;

    [SerializeField] bool m_allowKillKey;

    private int m_currentHealth;
    //private bool m_dead;
    private Transform m_transformJustDamaged;
    private AudioClipBucket m_audioClipBucket;
    private Rigidbody m_rigidBody;
    private bool m_inWater;
    private bool m_crashedOnGround;
    private GameObject[] m_activeFireAndSmoke;
    private float m_originalFireParticleLifetime;
    private float m_originalFireParticleEmissionRate;
    private float m_originalSmokeParticleLifetime;
    private float m_originalSmokeParticleEmissionRate;
    private float m_spinRate;
    private bool m_canTakeDamage = true;


    void Awake()
    {
        PlayerDead = false;

        m_currentHealth = m_startingHealth;
        m_audioClipBucket = GetComponentInParent<AudioClipBucket>();
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


    public void Damage(int damage)
    {
        if (damage >= m_minDamageForReset && !m_inWater)
            InstantiateExplosion(transform.position);

        if (!m_canTakeDamage || m_invulnerable || PlayerDead)
            return;

        if (damage >= m_minDamageForReset)
            DisableBriefly();

        m_currentHealth -= damage;

        float scaledDamage = damage * 0.1f;
        float magnitude = scaledDamage * m_cameraShakeMagnitude;
        float duration = scaledDamage * m_cameraShakeDuration;

        //print(string.Format("{0} damaged by {1}, current health = {2}", name, damage, m_currentHealth));

        if (m_currentHealth <= 0)
            Dead("");
        else
            EventManager.TriggerEvent(TwoFloatsEventName.ShakeCamera, magnitude, duration);
    }


    public void DisableBriefly()
    {
        //print(string.Format("{0} diabled for {1}s", name, m_canDamageResetTime));
        m_canTakeDamage = false;
        StartCoroutine(ResetTransformJustDamaged());
    }


    void Update()
    {
        if (m_allowKillKey && Input.GetKeyDown(KeyCode.Q))
            Damage(m_startingHealth);
    }


    void FixedUpdate()
    {
        if (!PlayerDead || m_rigidBody == null
            || m_rigidBody.isKinematic || m_inWater || m_crashedOnGround)
            return;

        float rotationZ = m_rigidBody.rotation.eulerAngles.z;
        var rotation = Quaternion.LookRotation(m_rigidBody.velocity);
        var eulerAngles = rotation.eulerAngles;
        eulerAngles.z = rotationZ += m_spinRate * Time.deltaTime;
        rotation = Quaternion.Euler(eulerAngles);

        m_rigidBody.MoveRotation(rotation);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.Bullet)
            || (m_transformJustDamaged != null && m_transformJustDamaged == other.transform))
            return;

        var otherDamageScript = other.gameObject.GetComponentInParent<IDamageable>();

        if (otherDamageScript != null)
        {
            //print(string.Format("{0} causes {1} damage to {2}", name, m_damageCausedToOthers, other.name));
            otherDamageScript.Damage(m_damageCausedToOthers);
        }    

        if (other.CompareTag(Tags.Water) && !m_inWater)
        {
            EnterWater();

            if (m_waterSplash != null)
                InstantiateWaterSplash(transform.position);
        }

        if (!m_invulnerable && (other.CompareTag(Tags.Ground) || other.CompareTag(Tags.Water)))
            Dead(other.tag);
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


    private void InstantiateFireAndSmoke()
    {
        m_activeFireAndSmoke = new GameObject[m_fireSpawnPoints.Length];

        for (int j = 0; j < m_fireSpawnPoints.Length; j++)
        {
            var spawnPoint = m_fireSpawnPoints[j].position;
            m_activeFireAndSmoke[j] = (GameObject) Instantiate(m_fireAndSmoke, spawnPoint, m_fireAndSmoke.transform.rotation);
            m_activeFireAndSmoke[j].transform.parent = transform;

            var particleSystems = m_activeFireAndSmoke[j].GetComponentsInChildren<ParticleSystem>();

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


    void OnCollisionEnter(Collision col)
    {
        if (m_inWater || m_crashedOnGround)
            return;

        if (col.gameObject.CompareTag(Tags.Ground) && m_explosion != null)
        {
            //print(name + " crashed into ground: " + col.contacts[0].point);
            m_crashedOnGround = true;

            InstantiateExplosion(col.contacts[0].point);

            m_rigidBody.velocity *= 0.1f;
            m_rigidBody.drag = 1f;

            for (int j = 0; j < m_activeFireAndSmoke.Length; j++)
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
        m_rigidBody.drag = m_rigidBodyDragInWater;
    }


    private IEnumerator ResetTransformJustDamaged()
    {
        yield return new WaitForSeconds(m_canDamageResetTime);

        m_canTakeDamage = true;
        m_transformJustDamaged = null;
    }


    private void Dead(string colliderTag)
    {
        if (PlayerDead)
            return;

        //print(colliderTag);
        //m_dead = true;
        PlayerDead = true;

        EventManager.TriggerEvent(StringEventName.PlayerDead, colliderTag);

        if (m_explosion != null && !m_inWater)
            InstantiateExplosion(transform.position);

        if (m_fireAndSmoke != null && !m_inWater)
            InstantiateFireAndSmoke();

        for (int i = 0; i < m_objectsToDetatchOnDeath.Length; i++)
        {
            var objectToDetatch = m_objectsToDetatchOnDeath[i];
            if (objectToDetatch != null)
                objectToDetatch.parent = null;
        }

        m_currentHealth = 0;

        if (m_aliveModel != null && m_deadModel != null)
        {
            m_aliveModel.SetActive(false);
            m_deadModel.SetActive(true);
        }
        //else if (!m_becomePhysicsObjectOnDeath)
        //    Destroy(gameObject, 0.05f);

        if (m_rigidBody != null)
            BecomePhysicsObject();
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


    public bool IsDead
    {
        get { return PlayerDead; }
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
        get { return true; }
    }


    public bool IsInWater
    {
        get { return m_inWater; }
    }


    public bool IsCrashedOnGround
    {
        get { return m_crashedOnGround; }
    }
}
