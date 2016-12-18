using UnityEngine;
using System.Collections;

public class CrystalEnemySpawner : MonoBehaviour {
	private EnemyHealth myHealth;
	public GameObject enemyToSpawn;

	IEnumerator EnemySpawn() {
		while(myHealth.IsDead == false) {
			yield return new WaitForSeconds(Random.Range(4.0f, 8.0f));
			GameObject tempGO = (GameObject)GameObject.Instantiate(enemyToSpawn,
				transform.position, transform.rotation);
			SelfDestructTime sdtScript = tempGO.GetComponent<SelfDestructTime>();
			sdtScript.enabled = true;
		}
	}

	// Use this for initialization
	void Start () {
		myHealth = GetComponentInParent<EnemyHealth>();

		StartCoroutine(EnemySpawn());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
