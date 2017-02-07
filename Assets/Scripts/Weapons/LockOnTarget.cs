using UnityEngine;
using System.Collections;

public class LockOnTarget : MonoBehaviour
{
	public GameObject chaseTarget;
	public GameObject childTrailOutlastsExplosion;
	public ParticleSystem particles;

	[HideInInspector]
	public float rocketSpeed = 30.0f;

	private int m_rocketDamage = 100;
	private float rocketAcceleration = 100.0f;
	private float maxSpeed = 250.0f;

	void Start()
	{
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Ignore Raycast"),
			LayerMask.NameToLayer("Player weapon"));
		StartCoroutine(LimitLifetime());
	}

	IEnumerator LimitLifetime()
	{
		yield return new WaitForSeconds(13.0f);
		SelfDestruct();
	}

	void Update()
	{
		rocketSpeed += rocketAcceleration * Time.deltaTime;
		if(rocketSpeed > maxSpeed)
		{
			rocketSpeed = maxSpeed;
		}
		transform.position += transform.forward * Time.deltaTime * rocketSpeed;
	}

	void FixedUpdate()
	{ // here to avoid Slerp result being affected by framerate
		if(chaseTarget != null)
		{
			float distNow = Vector3.Distance(chaseTarget.transform.position, transform.position);
			transform.rotation =
				Quaternion.Slerp(transform.rotation,
				Quaternion.LookRotation(chaseTarget.transform.position - transform.position),
					(distNow < 30.0f ? 0.5f : 0.15f)); // zero in for final connection, avoid circling it
		}
		else
		{
			Debug.Log("No tar");

			SelfDestruct();
		}
	}

	void SelfDestruct()
	{
		var explosion = Instantiate(particles);
		explosion.transform.position = transform.position;

		var explosionAudio = explosion.gameObject.GetComponent<ExplosionAudioManager>();

		float clipLength = explosion.startLifetime * 3f;

		if(explosionAudio != null)
			clipLength = explosionAudio.ClipLength;

		float lifetime = Mathf.Max(clipLength, explosion.startLifetime);

		Destroy(explosion.gameObject, lifetime);

		childTrailOutlastsExplosion.transform.SetParent(null);
		Destroy(childTrailOutlastsExplosion, 7.0f);
		Destroy(gameObject);
	}

	void OnTriggerStay(Collider other)
	{
		IDamageable damageScript = other.gameObject.GetComponentInParent<IDamageable>();

		if(damageScript != null)
		{
			damageScript.Damage(m_rocketDamage);
			m_rocketDamage = 0; //Rocket hits multiple times (bug). Workaround: rocket does 0 damage on multi hits after 1st.
		}

		SelfDestruct();
	}

	void OnCollisionEnter(Collision collInfo)
	{
		SelfDestruct();
	}

	public void setDamage(int damage)
	{
		m_rocketDamage = damage;
	}
}
