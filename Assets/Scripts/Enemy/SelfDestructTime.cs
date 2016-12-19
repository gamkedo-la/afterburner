using UnityEngine;
using System.Collections;

public class SelfDestructTime : MonoBehaviour {
	EnemyHealth myHealth;

	// Use this for initialization
	void Start () {
		myHealth = GetComponent<EnemyHealth>();
		StartCoroutine(DestroySelfOnDelay());
	}
	
	IEnumerator DestroySelfOnDelay() {
		yield return new WaitForSeconds( Random.Range(10.0f, 20.0f) );
		myHealth.Damage(999999);
	}
}
