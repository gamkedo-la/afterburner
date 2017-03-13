using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class PlayerShootingInput : MonoBehaviour
{
	private static readonly string Shoot = "Shoot", Missile = "Rocket";

	private DisplayTarget m_targetInfo;
	private FlyingControl m_flyingControlScript;
	private ShootingControl m_shootingControlScript;

	private Text missileAmmoText;

	private float lockOnTimer;
	private Transform playerTransform;
	private Transform target;

	void Start()
	{
		m_shootingControlScript = GetComponent<ShootingControl>();
		m_flyingControlScript = GetComponent<FlyingControl>();

		GameObject targetTrackerGO = (GameObject)GameObject.Find("Target Canvas");
		if(targetTrackerGO)
		{
			m_targetInfo = targetTrackerGO.GetComponent<DisplayTarget>();
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
				missileAmmoText = missileAmmoGO.GetComponent<Text>();
				MissileAmmoUpdate();
			}
		}

		lockOnTimer = 0f;
		playerTransform = FindObjectOfType<PlayerFlyingInput>().GetComponent<Transform>();
	}

	void MissileAmmoUpdate()
	{
		Debug.Log("Ammo update: " + m_shootingControlScript.getMissileAmmo());
		missileAmmoText.text = "Missile:" + m_shootingControlScript.getMissileAmmo() + " (E)";
	}

	void Update()
	{

		if((Input.GetAxisRaw(Shoot) == 1 || Input.GetAxisRaw(Shoot) == -1) && Time.timeScale > 0)
		{
			m_shootingControlScript.Shoot();
		}

		if((Input.GetAxisRaw(Missile) == 1 || Input.GetAxisRaw(Missile) == -1) && Time.timeScale > 0 && lockedOn())
		{
			GameObject missile = m_shootingControlScript.Launch();
			if(missile != null)
			{
				MissileAmmoUpdate(); //Update ammo GUI number

				//Pass the missile the current target
				LockOnTarget lotScript = missile.GetComponent<LockOnTarget>();

				if(m_flyingControlScript != null)
					lotScript.rocketSpeed = m_flyingControlScript.ForwardVelocity.magnitude;

				if(m_targetInfo)
				{
					lotScript.chaseTarget = m_targetInfo.returnCurrentTarget();
				}
				//Target is passed
			}
		}
	}

	void LateUpdate()
	{
		if(m_targetInfo != null)
		{
			target = m_targetInfo.returnCurrentTarget().transform;
		}

		if(target != null)
		{
			float targetAngle = Quaternion.Angle(
				Quaternion.LookRotation(target.transform.position - playerTransform.position),
				Quaternion.LookRotation(playerTransform.forward));

			float targetDistance = Vector3.Distance(target.transform.position, playerTransform.position);

			if(targetAngle < m_shootingControlScript.lockOnAngle && targetDistance < m_shootingControlScript.lockOnRange && m_shootingControlScript.getMissileAmmo() > 0)
			{
				lockOnTimer += Time.deltaTime;
			}
			else
			{
				lockOnTimer = 0f;
			}
		}
	}

	public bool lockedOn()
	{
		return lockOnTimer > m_shootingControlScript.timeToLockOn;
	}
}
