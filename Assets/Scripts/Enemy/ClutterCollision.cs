using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClutterCollision : MonoBehaviour
{
	[SerializeField]
	int m_damageCausedToOthers = 100;

	[SerializeField]
	GameObject m_aliveModel;

	private AudioClipBucket m_audioClipBucket;
	private Rigidbody m_rigidBody;

	/*
	void Awake()
	{
		m_audioClipBucket = GetComponent<AudioClipBucket>();
		m_rigidBody = GetComponent<Rigidbody>();

		if (m_aliveModel != null)
		{
			m_aliveModel.SetActive(true);
		}
	}
	*/

	void OnTriggerEnter(Collider other)
	{
		var otherDamageScript = other.gameObject.GetComponentInParent<IDamageable>();

		if (otherDamageScript != null)
		{
			otherDamageScript.Damage(m_damageCausedToOthers);
		}
	}
}
