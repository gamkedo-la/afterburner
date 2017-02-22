using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlyingControl), typeof(EnemyHealth))]
public class EnemyAircraftAiInput : MonoBehaviour
{
	[SerializeField] float m_playerInRangeAttackThreshold = 500f;
	[SerializeField] float m_fleeHealthProportion = 0.3f;
	[SerializeField] float m_evadeMaxDotThreshold = -0.5f;
	[SerializeField] float m_directionControlsSensitivity = 0.5f;
	[SerializeField] float m_thrustControlsSensitivity = 0.5f;
	[SerializeField] float m_decisionRate = 0.2f;
	[SerializeField] float m_chaseBankSensitivity = 1f;
	[SerializeField] float m_evadeMaxBankAngle = 90f;
	[SerializeField] float m_chaseMaxBankAngle = 90f;
	[SerializeField] Vector2 m_evadeChangeTimeMinMax = new Vector2(0.5f, 4f);
	[SerializeField] Vector2 m_pitchAngleMinMax = new Vector2(-45f, 45f);
	[SerializeField] Vector2 m_bankAngleMinMaxForPitching = new Vector2(-45f, 45f);
	[SerializeField] Vector2 m_altitudeMinMax = new Vector2(100f, 1000f);
	[SerializeField] Aircraft_Whiskers m_whiskers;
	[SerializeField] float m_postAvoidDelay = 0.4f; //elapsed time to keep avoiding after there are no whisker collisions
	[SerializeField] bool useNewFiringSolution = true;

	private WaitForSeconds m_waitTime;

	[Header("Script references")]
	private FlyingControl m_flyingControlScript;
	private EnemyHealth m_health;
	private Transform m_player;

	[Header("Spawn values")]
	//private Vector3 m_spawnPoint;
	//private Quaternion m_spawnRotation;
	private float m_spawnBankAngle;
	private float m_patrolSpeed;
	private int m_fleeHealth;

	[Header("Decision making values")]
	private State m_state = State.Patrol;
	private Vector3 m_playerDirection;
	private bool m_playerInRange;
	private float m_dotThisForwardToPlayer;

	[Header("World space angles to player")]
	private float m_forwardAngleToPlayer;
	private float m_playerVerticalAngle;

	[Header("Pitch angle values")]
	private float m_pitchAngleToPlayer;
	private float m_pitchAngle;

	[Header("Bank angle values")]
	private float m_bankAngle;
	private float m_bankMagnitude;
	private float m_bankAngleToAimFor;
	private float m_bankAngleFromRightLimit;
	private float m_bankAngleFromLeftLimit;
	private bool m_turnRight;

	[Header("Evade decision values")]
	private float m_evadeChangeTime;
	private float m_timeSinceEvadeChange;

	[Header("Control values")]
	private float m_v;
	private float m_h;
	private float m_a;

	[Header("Whisker values")]
	private AircraftWhiskerInfo m_cWhiskerData;
	private bool m_bDoAvoid; //bool that tracks if state should be avoid, true during whisker collision
	private bool m_bAvoidResetWait; //flag to track if we are already waiting to reset, in case we get more false messages for whisker collisions

	private float bulletSpeed;

	void Awake()
	{
		m_flyingControlScript = GetComponent<FlyingControl>();
		m_health = GetComponent<EnemyHealth>();

		//if whiskers are not set in the inspector, see if they are attached to this component
		if(!m_whiskers)
			m_whiskers = GetComponent<Aircraft_Whiskers>();

		var playerObject = GameObject.FindGameObjectWithTag(Tags.Player);

		if(playerObject != null)
		{
			m_player = playerObject.transform;
			EnemyAIUtils.playerFlyingStats = playerObject.GetComponent<FlyingControl>();
		}

		m_waitTime = new WaitForSeconds(m_decisionRate);

		ShootingControl shootingControl = GetComponentInChildren<ShootingControl>();
		BulletFlight bullet = shootingControl.getBullet();
		bulletSpeed = bullet.getSpeed();
	}


	void Start()
	{
		//m_spawnPoint = transform.position;
		//m_spawnRotation = transform.rotation;
		m_spawnBankAngle = EnemyAIUtils.StandardiseAngle(-transform.rotation.eulerAngles.z);
		m_fleeHealth = Mathf.RoundToInt(m_health.CurrentHealth * m_fleeHealthProportion);
		m_patrolSpeed = m_flyingControlScript.ForwardSpeed;
		m_cWhiskerData = new AircraftWhiskerInfo(Vector3.zero, false, false);

		Subscribe();

		if(m_player != null)
			StartCoroutine(MakeDecisions());
	}

	//Subscribe to events, currently just to subscribe to the WhiskerCollision event 
	private void Subscribe()
	{
		if(m_whiskers)
		{
			m_whiskers.WhiskerCollisionSet += OnWhiskerSet;
		}
	}

	//Unsubscribe from the subscribed events, this should probably occur on death, before the death animation
	// don't need to clean up if the whiskers are attached to this game object or a child of this game object, 
	private void Unsubscribe()
	{
		if(m_whiskers)
		{
			m_whiskers.WhiskerCollisionSet -= OnWhiskerSet;
		}
	}

	private IEnumerator MakeDecisions()
	{
		float randomTime = Random.Range(0f, m_decisionRate);
		yield return new WaitForSeconds(randomTime);

		while(true)
		{
			CheckOrientation();

			if(PlayerHealth.PlayerDead || m_player == null)
				m_state = State.Patrol;
			else
			{
				CheckPlayerRange();
				CheckHealth();
			}

			CheckWhiskers();

			switch(m_state)
			{
				case (State.Patrol):
					UpdatePatrol();
					break;

				case (State.Chase):
					UpdateChase();
					break;

				case (State.Evade):
					UpdateEvade();
					break;

				case (State.Flee):
					UpdateFlee();
					break;

				case (State.Avoid):
					UpdateAvoid();
					break;
			}

			if(m_state != State.Avoid)
				CheckAltitude();

			yield return m_waitTime;
		}
	}


	void Update()
	{
		m_flyingControlScript.PitchAndRollInput(m_v * m_directionControlsSensitivity, m_h * m_directionControlsSensitivity);
		m_flyingControlScript.ThrustInput(m_a * m_directionControlsSensitivity);
	}


	private void CheckHealth()
	{
		float health = m_health.CurrentHealth;

		if(health <= m_fleeHealth
				&& m_playerInRange)
			m_state = State.Flee;
	}

	private void CheckWhiskers()
	{
		if(m_bDoAvoid)
		{
			//Whiskers are colliding with something. Set state
			m_state = State.Avoid;
		}
	}

	private void CheckOrientation()
	{
		m_bankAngle = EnemyAIUtils.StandardiseAngle(transform.rotation.eulerAngles.z);

		if(m_bankAngle > 180f)
			m_bankAngle -= 360f;

		m_bankAngle = -m_bankAngle;

		var forwardOnGround = new Vector2(transform.forward.x, transform.forward.z);
		m_pitchAngle = Mathf.Atan2(transform.forward.y, forwardOnGround.magnitude);

		m_pitchAngle *= -Mathf.Rad2Deg;

		if(m_player == null)
			return;

		m_playerDirection = m_player.position - transform.position;

		Vector3 targetDirection = m_playerDirection;

		if(useNewFiringSolution)
		{
			targetDirection = EnemyAIUtils.calculateLeadDirection(m_playerDirection, bulletSpeed, m_decisionRate);
			if(targetDirection == Vector3.zero)
			{
				targetDirection = m_playerDirection;
			}
		}

		var playerDirectionOnGround = new Vector2(targetDirection.x, targetDirection.z);
		var playerDirectionVertical = new Vector2(playerDirectionOnGround.magnitude, targetDirection.y);

		var playerDirectionNormalized = targetDirection.normalized;

		m_dotThisForwardToPlayer = Vector3.Dot(transform.forward, playerDirectionNormalized);
		//m_dotThisUpToPlayer = Vector3.Dot(transform.up, playerDirectionNormalized);
		//m_dotThisRightToPlayer = Vector3.Dot(transform.right, playerDirectionNormalized);
		//m_dotThisUpToUp = Vector3.Dot(transform.up, Vector3.up);
		//m_dotThisRightToUp = Vector3.Dot(transform.right, Vector3.up);
		//m_dotUpToPlayer = Vector3.Dot(Vector3.up, playerDirectionNormalized);

		float angleForwards = Mathf.Atan2(forwardOnGround.x, forwardOnGround.y);
		float anglePlayer = Mathf.Atan2(playerDirectionOnGround.x, playerDirectionOnGround.y);

		m_forwardAngleToPlayer = EnemyAIUtils.StandardiseAngle(Mathf.Rad2Deg * (anglePlayer - angleForwards));

		m_playerVerticalAngle = -Mathf.Rad2Deg * Mathf.Atan2(playerDirectionVertical.y, playerDirectionVertical.x);

		m_pitchAngleToPlayer = m_playerVerticalAngle - m_pitchAngle;

		//Debug.DrawLine(transform.position, targetDirection + transform.position, Color.red, 0.2f);
	}



	private void CheckPlayerRange()
	{
		float range = m_playerDirection.magnitude;

		m_playerInRange = range <= m_playerInRangeAttackThreshold;

		if(!m_playerInRange)
		{
			m_state = State.Patrol;
		}
		else
		{
			m_state = m_dotThisForwardToPlayer > m_evadeMaxDotThreshold ? State.Chase : State.Evade;
		}
	}


	private void UpdatePatrol()
	{
		m_a = Mathf.Clamp(m_patrolSpeed - m_flyingControlScript.ForwardSpeed, -1f, 1f);

		m_bankAngleToAimFor = m_spawnBankAngle;

		SetHorizontal();
		FlattenPitch();
	}


	private void UpdateChase()
	{
		BankAngleToAimFor(m_forwardAngleToPlayer);
		SetHorizontal();
		PitchToAimAtPlayer();
	}


	private void BankAngleToAimFor(float angleToTurn)
	{
		float maxAngleToTurn = Mathf.Min(m_chaseMaxBankAngle, Mathf.Abs(2f * angleToTurn));

		m_bankAngleToAimFor = Mathf.Sign(angleToTurn) * maxAngleToTurn;
	}


	private void SetHorizontal()
	{
		float bankAngleToChange = m_bankAngleToAimFor - m_bankAngle;

		m_h = m_chaseBankSensitivity * bankAngleToChange / m_flyingControlScript.turnRate;
		m_h = Mathf.Clamp(m_h, -1f, 1f);
	}


	private void PitchToAimAtPlayer()
	{
		if(m_bankAngle < m_bankAngleMinMaxForPitching.x || m_bankAngle > m_bankAngleMinMaxForPitching.y)
		{
			m_v = 0;
			return;
		}

		m_v = m_pitchAngleToPlayer * 0.1f;

		if(m_v > 0 && m_pitchAngle > m_pitchAngleMinMax.y)
			m_v = 0;
		else if(m_v < 0 && m_pitchAngle < m_pitchAngleMinMax.x)
			m_v = 0;

		m_v = Mathf.Clamp(m_v, -1f, 1f);
	}

	private void PitchToAvoid(bool pitchUp)
	{
		//if (m_bankAngle < m_bankAngleMinMaxForPitching.x || m_bankAngle > m_bankAngleMinMaxForPitching.y)
		//{
		//    print("returning from pitch to avoid early 0");
		//    m_v = 0;
		//    return;
		//}

		if(pitchUp)
			m_v = -1.0f; //change this to increments for smoothing?
		else
			m_v = 1.0f;

		//if (m_v > 0 && m_pitchAngle > m_pitchAngleMinMax.y)
		//    m_v = 0;
		//else if (m_v < 0 && m_pitchAngle < m_pitchAngleMinMax.x)
		//    m_v = 0;

		m_v = Mathf.Clamp(m_v, -1.0f, 1.0f);
	}

	private void CheckAltitude()
	{
		if(transform.position.y < m_altitudeMinMax.x)
		{
			FlattenRoll();
			m_v = -1f;

			if(m_pitchAngle < m_pitchAngleMinMax.x)
				m_v = 0;
		}
		else if(transform.position.y > m_altitudeMinMax.y)
		{
			FlattenRoll();
			m_v = 1f;

			if(m_pitchAngle > m_pitchAngleMinMax.y)
				m_v = 0;
		}
	}


	private void UpdateEvade()
	{
		FlattenPitch();

		m_a = Mathf.Clamp(m_patrolSpeed - m_flyingControlScript.ForwardSpeed, -1f, 1f);

		m_timeSinceEvadeChange += m_decisionRate;

		if(m_timeSinceEvadeChange > m_evadeChangeTime)
		{
			int choice = Random.Range(0, 2);
			m_turnRight = choice > 0;

			m_evadeChangeTime = Random.Range(m_evadeChangeTimeMinMax.x, m_evadeChangeTimeMinMax.y);
			m_timeSinceEvadeChange = 0f;
		}

		Bank();
	}


	private void Bank()
	{
		CalculateBankMagnitude(m_evadeMaxBankAngle);

		if(m_turnRight)
			BankToRight();
		else
			BankToLeft();
	}


	private void CalculateBankMagnitude(float maxBankAngle)
	{
		m_bankAngleFromRightLimit = maxBankAngle - m_bankAngle;
		m_bankAngleFromLeftLimit = m_bankAngle + maxBankAngle;

		m_bankMagnitude = m_turnRight ? m_bankAngleFromRightLimit / 90f : m_bankAngleFromLeftLimit / 90f;
	}


	private void BankToLeft()
	{
		m_h = -m_bankMagnitude;

		m_h = Mathf.Clamp(m_h, -1f, 1f);
	}


	private void BankToRight()
	{
		m_h = m_bankMagnitude;

		m_h = Mathf.Clamp(m_h, -1f, 1f);
	}


	private void UpdateFlee()
	{
		if(m_flyingControlScript.ForwardSpeed < m_flyingControlScript.MaxForwardSpeed)
			m_a = m_thrustControlsSensitivity;
		else
			m_a = 0f;

		FlattenPitch();
		FlattenRoll();
	}

	private void UpdateAvoid()
	{
		float angleToUp = Vector3.Angle(m_cWhiskerData.safeVector, transform.up);

		if(angleToUp <= 90.0f)
		{
			//closer to the up vector, rotate towards transform.up
			float rollAmount = -angleToUp;

			if(Vector3.Angle(m_cWhiskerData.safeVector, transform.right) < 90)
				rollAmount = -rollAmount;
			rollAmount += m_bankAngle;

			BankAngleToAimFor(rollAmount);
			SetHorizontal();

			if(angleToUp <= 45.0f) //Start pitching when within 45 degrees of the desired safe vector
				PitchToAvoid(true);
		}
		else
		{
			float angleToDown = Vector3.Angle(m_cWhiskerData.safeVector, -transform.up);
			if(angleToDown <= 90.0f)
			{
				//closer to the down vector, rotate towards transform.up
				float rollAmount = -angleToDown;

				if(Vector3.Angle(m_cWhiskerData.safeVector, transform.right) > 90)
					rollAmount = -rollAmount;

				rollAmount += m_bankAngle;

				BankAngleToAimFor(rollAmount);
				SetHorizontal();

				if(angleToDown <= 45.0f) //Start pitching when within 45 degrees of the desired safe vector
					PitchToAvoid(false);
			}
		}
	}

	private void FlattenPitch()
	{
		if(m_bankAngle < m_bankAngleMinMaxForPitching.x || m_bankAngle > m_bankAngleMinMaxForPitching.y)
		{
			m_v = 0;
			return;
		}

		var forward = transform.forward;
		float dotUp = Vector3.Dot(Vector3.up, forward);
		m_v = dotUp;
	}


	private void FlattenRoll()
	{
		BankAngleToAimFor(0);
		SetHorizontal();
	}

	private void OnWhiskerSet(AircraftWhiskerInfo info)
	{

		m_cWhiskerData = info;
		if(info.isActive)
		{
			m_bDoAvoid = true;
			StopCoroutine("StartResetWait"); //if whiskers become active during reset wait, then stop the routine
			m_bAvoidResetWait = false;
		}
		if(info.isActive == false && m_bDoAvoid && !m_bAvoidResetWait)
		{
			m_bAvoidResetWait = true;
			StartCoroutine(StartResetWait());
		}
	}

	//Allow the plane to keep tracking in the current direction until post avoid delay is over, this prevents the plane from getting extremely close to object and blowing up
	private IEnumerator StartResetWait()
	{
		yield return new WaitForSeconds(m_postAvoidDelay);
		m_bAvoidResetWait = false;
		m_bDoAvoid = false;
	}

	private enum State
	{
		Patrol = 0,
		Chase = 1,
		Evade = 2,
		Flee = 3,
		Avoid = 4,
	}
}