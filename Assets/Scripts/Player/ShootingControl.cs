using UnityEngine;
using System.Collections;

public class ShootingControl : MonoBehaviour
{
	[SerializeField]
	BulletFlight m_bullet;
	[SerializeField]
	Transform[] m_bulletSpawnPoints;
	[SerializeField]
	Transform[] m_missileSpawnPoints;
	[SerializeField]
	float m_bulletCooldown = 0.15f, m_missileCooldown = 1.0f;
	[SerializeField]
	float m_muzzleFlashTime = 0.05f;
	[SerializeField]
	bool m_shakeCamera;
	[SerializeField]
	float m_cameraShakeMagnitude = 0.2f;
	[SerializeField]
	float m_cameraShakeDuration = 0.1f;
	[SerializeField]
	Vector2 m_gunShotPitchMultiplierMinMax = new Vector2(0.8f, 1.2f);

	private float m_timeSinceBulletFired, m_timeSinceMissileFired;
	private FlyingControl m_flyingControlScript;
	private int m_spawnPointIndex;
	private int m_missileSpawnPointIndex;
	private int m_numSpawnPoints;
	private int m_numMissileSpawnPoints;
	private WaitForSeconds m_muzzleFlashWait;
	private GameObject[] m_muzzleFlashes;
	private AudioSource[] m_gunShotAudioSources;
	private float[] m_gunShotAudioPitches;
	private GameObject m_missilePrefab;

	[SerializeField]
	int m_missileAmmo = 8, missileDamage = 50;
	public float timeToLockOn = 2f, lockOnAngle = 20f, lockOnRange = 400f;
	

	void Awake()
	{
		m_flyingControlScript = GetComponent<FlyingControl>();
		m_numSpawnPoints = m_bulletSpawnPoints.Length;
		m_muzzleFlashWait = new WaitForSeconds(m_muzzleFlashTime);
	}


	void Start()
	{
		m_muzzleFlashes = new GameObject[m_bulletSpawnPoints.Length];
		m_gunShotAudioSources = new AudioSource[m_bulletSpawnPoints.Length];
		m_gunShotAudioPitches = new float[m_bulletSpawnPoints.Length];

		for(int i = 0; i < m_bulletSpawnPoints.Length; i++)
		{
			var spawnPoint = m_bulletSpawnPoints[i];

			var muzzleFlash = spawnPoint.GetComponentInChildren<MeshRenderer>();

			if(muzzleFlash != null)
			{
				muzzleFlash.gameObject.SetActive(false);
				m_muzzleFlashes[i] = muzzleFlash.gameObject;
			}

			var gunShotAudio = spawnPoint.GetComponentInChildren<AudioSource>();

			if(gunShotAudio != null)
			{
				m_gunShotAudioSources[i] = gunShotAudio;
				m_gunShotAudioPitches[i] = gunShotAudio.pitch;
			}
		}

		m_spawnPointIndex = 0;

		if(m_missileSpawnPoints != null) //Prevents errors on menus
		{
			m_numMissileSpawnPoints = m_missileSpawnPoints.Length;
			m_missileSpawnPointIndex = 0;
			m_missilePrefab = (GameObject)Resources.Load("GuidedMissile");
		}
	}


	void Update()
	{
		m_timeSinceBulletFired += Time.deltaTime;
		m_timeSinceMissileFired += Time.deltaTime;
	}


	public void Shoot()
	{
		if(m_timeSinceBulletFired > m_bulletCooldown)
		{
			if(m_shakeCamera)
				EventManager.TriggerEvent(TwoFloatsEventName.ShakeCamera, m_cameraShakeMagnitude, m_cameraShakeDuration);

			m_timeSinceBulletFired = 0;
			var bullet = Instantiate(m_bullet);
			bullet.transform.parent = transform;

			var spawnPoint = m_bulletSpawnPoints[m_spawnPointIndex];

			bullet.transform.position = spawnPoint.position;

			bullet.transform.rotation = spawnPoint.rotation;

			if(m_flyingControlScript != null)
				bullet.SetInitialVelocity(m_flyingControlScript.ForwardVelocity);
			else
				bullet.SetInitialVelocity(Vector3.zero);

			var muzzleFlash = m_muzzleFlashes[m_spawnPointIndex];

			if(muzzleFlash != null)
			{
				muzzleFlash.SetActive(true);

				StartCoroutine(TurnOffQuad(muzzleFlash));
			}

			var gunShotAudio = m_gunShotAudioSources[m_spawnPointIndex];

			if(gunShotAudio != null && gunShotAudio.clip != null)
			{
				float originalPitch = m_gunShotAudioPitches[m_spawnPointIndex];
				gunShotAudio.pitch = originalPitch * Random.Range(m_gunShotPitchMultiplierMinMax.x, m_gunShotPitchMultiplierMinMax.y);
				gunShotAudio.PlayOneShot(gunShotAudio.clip);
			}

			m_spawnPointIndex++;
			m_spawnPointIndex = m_spawnPointIndex % m_numSpawnPoints;
		}
	}

	public GameObject Launch()
	{
		Debug.Log("Launch");
		if(m_missileAmmo > 0 && m_timeSinceMissileFired > m_missileCooldown)
		{
			Transform spawnPoint = m_missileSpawnPoints[m_missileSpawnPointIndex];

			m_missileAmmo--;
			m_timeSinceMissileFired = 0.0f;

			GameObject missile = (GameObject)GameObject.Instantiate(m_missilePrefab,
				spawnPoint.position, spawnPoint.rotation);

			m_missileSpawnPointIndex++;
			m_missileSpawnPointIndex = m_missileSpawnPointIndex % m_numMissileSpawnPoints;

			//Set missile damage
			missile.GetComponent<LockOnTarget>().setDamage(missileDamage);

			return missile;
		}
		return null;
	}

	public int getMissileAmmo()
	{
		return m_missileAmmo;
	}

	private IEnumerator TurnOffQuad(GameObject muzzleFlashObject)
	{
		yield return m_muzzleFlashWait;

		muzzleFlashObject.SetActive(false);
	}
}
