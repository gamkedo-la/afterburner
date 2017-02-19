using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GunTurretControl))]
public class EnemyGunTurretAiInput : MonoBehaviour
{
	[SerializeField]
	Transform m_gunBarrelTransform;
	[SerializeField]
	float m_decisionRate = 0.2f;
	[SerializeField]
	float m_playerInRangeAttackThreshold = 500f;
	[SerializeField]
	bool useNewFiringSolution = true;

	private GunTurretControl m_gunTurretControlScript;
	private WaitForSeconds m_waitTime;
	private Transform m_player;
	private Rigidbody playerRigidbody;

	private State m_state = State.Rest;
	private Vector3 m_playerDirection;

	private float m_playerVerticalAngle;
	private float m_playerHorizontalAngle;

	private float m_v;
	private float m_h;

	private float bulletSpeed;

	void Awake()
	{
		m_gunTurretControlScript = GetComponent<GunTurretControl>();

		var playerObject = GameObject.FindGameObjectWithTag(Tags.Player);

		if(playerObject != null)
		{
			m_player = playerObject.transform;
			playerRigidbody = playerObject.GetComponent<Rigidbody>();
		}

		m_waitTime = new WaitForSeconds(m_decisionRate);

		ShootingControl shootingControl = GetComponentInChildren<ShootingControl>();
		BulletFlight bullet = shootingControl.getBullet();
		bulletSpeed = bullet.getSpeed();
  }


	void Start()
	{
		if(m_player != null)
			StartCoroutine(MakeDecisions());
	}


	void Update()
	{
		m_gunTurretControlScript.Move(m_v, m_h);
	}


	private IEnumerator MakeDecisions()
	{
		float randomTime = Random.Range(0f, m_decisionRate);
		yield return new WaitForSeconds(randomTime);

		while(true)
		{
			CheckOrientation();

			switch(m_state)
			{
				case (State.Rest):
					UpdateRest();
					break;

				case (State.Attack):
					UpdateAttack();
					break;
			}

			yield return m_waitTime;
		}
	}


	private void CheckOrientation()
	{
		if(m_player == null)
		{
			m_state = State.Rest;
			return;
		}

		m_playerDirection = m_player.position - m_gunBarrelTransform.position;

		if(m_playerDirection.magnitude <= m_playerInRangeAttackThreshold)
			m_state = State.Attack;
		else
			m_state = State.Rest;
	}


	private static float StandardiseAngle(float angle)
	{
		float newAngle = (360f + angle) % 360f;

		if(newAngle > 180f)
			newAngle -= 360f;

		return newAngle;
	}


	private void UpdateRest()
	{
		m_v = 0f;
		m_h = 0f;
	}


	private void UpdateAttack()
	{
		Vector3 targetDirection = m_playerDirection;

    if(useNewFiringSolution)
		{

			targetDirection = calculateLeadDirection(m_playerDirection);
			if(targetDirection == Vector3.zero)
			{
				targetDirection = m_playerDirection;
      }
		}

		var forwardOnGround = new Vector2(m_gunBarrelTransform.forward.x, m_gunBarrelTransform.forward.z);
		var playerDirectionOnGround = new Vector2(targetDirection.x, targetDirection.z);
		var playerDirectionVertical = new Vector2(playerDirectionOnGround.magnitude, targetDirection.y);

		float angleForwards = Mathf.Atan2(forwardOnGround.x, forwardOnGround.y);
		float anglePlayer = Mathf.Atan2(playerDirectionOnGround.x, playerDirectionOnGround.y);

		m_playerHorizontalAngle = StandardiseAngle(Mathf.Rad2Deg * (anglePlayer - angleForwards));

		float playerVerticalAngle = Mathf.Atan2(playerDirectionVertical.y, playerDirectionVertical.x);
		float gunBarrelVerticalAngle = Mathf.Atan2(m_gunBarrelTransform.forward.y, forwardOnGround.magnitude);

		m_playerVerticalAngle = Mathf.Rad2Deg * (gunBarrelVerticalAngle - playerVerticalAngle);

		m_v = m_playerVerticalAngle;
		m_h = m_playerHorizontalAngle;

//		m_v = Mathf.Clamp(m_v, -1f, 1f);
	//	m_h = Mathf.Clamp(m_h, -1f, 1f);
	}

	private Vector3 calculateLeadDirection(Vector3 playerDirection)
	{
		//Aim in front of player to lead shot
		float timeToImpact = AimAhead(playerDirection, playerRigidbody.velocity, bulletSpeed);

		//If there is a possible collision time in the future rather than the past
		if(timeToImpact > 0f)
		{
			//Return point to aim at reletive to gun barrels
			return (m_player.position + timeToImpact * playerRigidbody.velocity) - m_gunBarrelTransform.position;
		}
		//Else return the zero vector
		return Vector3.zero;
	}

	// Calculate the time when we can hit a target with a bullet
	// Return a negative time if there is no solution
	protected float AimAhead(Vector3 delta, Vector3 vr, float muzzleV)
	{
		// Quadratic equation coefficients a*t^2 + b*t + c = 0
		float a = Vector3.Dot(vr, vr) - muzzleV * muzzleV;
		float b = 2f * Vector3.Dot(vr, delta);
		float c = Vector3.Dot(delta, delta);

		float det = b * b - 4f * a * c;

		// If the determinant is negative, then there is no solution
		if(det > 0f)
		{
			return 2f * c / (Mathf.Sqrt(det) - b);
		}
		else
		{
			return -1f;
		}
	}

	private enum State
	{
		Rest = 0,
		Attack = 1,
	}
}
