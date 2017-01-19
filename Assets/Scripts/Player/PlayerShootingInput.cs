using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class PlayerShootingInput : MonoBehaviour
{
	private static readonly string Shoot = "Shoot", Rocket = "Rocket";

	private DisplayTarget m_targetInfo;
	private GameObject m_rocketPrefab;
	private FlyingControl m_flyingControlScript;
	private ShootingControl m_shootingControlScript;

	private int m_rocketAmmo = 4;
	private Text rocketAmmoText;

	private float m_rocketCooldown = 1.0f;
	private float m_timeSinceRocketFired;
	private int m_numMissileSpawnPoints;
	private int m_missileSpawnPointIndex;
	[SerializeField]
	Transform[] m_missileSpawnPoints;


	void Start()
	{
		m_timeSinceRocketFired = 0.0f;
		m_shootingControlScript = GetComponent<ShootingControl>();
		m_flyingControlScript = GetComponent<FlyingControl>();

		GameObject targetTrackerGO = (GameObject)GameObject.Find("Target Canvas");
		if(targetTrackerGO)
		{
			m_targetInfo = targetTrackerGO.GetComponent<DisplayTarget>();
			m_rocketPrefab = (GameObject)Resources.Load("GuidedMissile");
		}
		GameObject missileAmmoGO = (GameObject)GameObject.Find("Missile Label");
		if(missileAmmoGO)
		{
			if(targetTrackerGO == null)
			{
				missileAmmoGO.SetActive(false);
			}
			else
			{
				rocketAmmoText = missileAmmoGO.GetComponent<Text>();
				RocketAmmoUpdate();
			}
		}
	}

	void Awake()
	{
		if(m_missileSpawnPoints != null) //Prevents errors on menus
		{
			m_numMissileSpawnPoints = m_missileSpawnPoints.Length;
			m_missileSpawnPointIndex = 0;
		}
  }

	void RocketAmmoUpdate()
	{
		rocketAmmoText.text = "Missile:" + m_rocketAmmo + " (E)";
	}

	void Update()
	{
		m_timeSinceRocketFired += Time.deltaTime;

		if((Input.GetAxisRaw(Shoot) == 1 || Input.GetAxisRaw(Shoot) == -1) && Time.timeScale > 0)
			m_shootingControlScript.Shoot();

		if(m_rocketAmmo > 0 && (Input.GetAxisRaw(Rocket) == 1 || Input.GetAxisRaw(Rocket) == -1) && Time.timeScale > 0
			&& (m_timeSinceRocketFired > m_rocketCooldown))
		{
			Transform spawnPoint = m_missileSpawnPoints[m_missileSpawnPointIndex];

			m_rocketAmmo--;
			RocketAmmoUpdate();
			m_timeSinceRocketFired = 0.0f;

			GameObject rocketGO = (GameObject)GameObject.Instantiate(m_rocketPrefab,
				spawnPoint.position, spawnPoint.rotation);
			LockOnTarget lotScript = rocketGO.GetComponent<LockOnTarget>();

			if(m_flyingControlScript != null)
				lotScript.rocketSpeed = m_flyingControlScript.ForwardVelocity.magnitude;

			if(m_targetInfo)
			{
				lotScript.chaseTarget = m_targetInfo.returnCurrentTarget();
			}

			m_missileSpawnPointIndex++;
			m_missileSpawnPointIndex = m_missileSpawnPointIndex % m_numMissileSpawnPoints;

		}
	}
}