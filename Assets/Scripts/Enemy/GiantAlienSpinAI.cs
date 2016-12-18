using UnityEngine;
using System.Collections;

public class GiantAlienSpinAI : MonoBehaviour {
	EnemyHealth myHealth;
	float rotY=0.0f;
	float spinRate;

	IEnumerator Rethink() {
		while(myHealth.IsDead == false) {
			bool wasNeg = (spinRate < 0.0f);
			spinRate = Random.Range(25.0f,70.0f);
			if(wasNeg == false) {
				spinRate *= -1.0f;
			}
			yield return new WaitForSeconds(Random.Range(1.6f, 4.4f));
		}
	}

	// Use this for initialization
	void Start () {
		myHealth = GetComponent<EnemyHealth>();

		if(Random.Range(0.0f, 1.0f) < 0.5f) {
			spinRate = -1.0f;
		} else {
			spinRate = 1.0f;
		}
		StartCoroutine(Rethink());
	}
	
	// Update is called once per frame
	void Update () {
		rotY += spinRate * Time.deltaTime;
		transform.rotation =
			Quaternion.AngleAxis(rotY, Vector3.up);
	}
}
