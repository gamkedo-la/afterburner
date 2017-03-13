using UnityEngine;
using System.Collections;

public class SelfDestructTime : MonoBehaviour {
	EnemyHealth myHealth;

	[SerializeField]
	GameObject model;
	private Material crystalAlienMaterial;
	float explosionTimer, flashRate;

	[SerializeField]
	float flashDuration, flashRateDelta, flashRateAcceleration;
	//Color color;

	// Use this for initialization
	void Start () {
		myHealth = GetComponent<EnemyHealth>();
		explosionTimer = Random.Range(20f, 25.0f);
		//explosionTimer = Random.Range(10.0f, 20.0f);

		crystalAlienMaterial = model.GetComponent<Renderer>().material;

		StartCoroutine(DestroySelfOnDelay());
		flashRate = 2.7f;
		//		color = Color.white;
	}

	void Update()
	{
		explosionTimer -= Time.deltaTime;
		if(explosionTimer < flashDuration)
		{
			flashRate = Mathf.Repeat((flashRate + 0.05f * flashRateDelta), 2);
			flashRateDelta += flashRateAcceleration;
			flashRateAcceleration += flashRateAcceleration * 0.01f;
			Debug.Log(flashRate);

			if(flashRate <= 1)
			{
				crystalAlienMaterial.SetColor("_Color", Color.Lerp(Color.red, Color.white, flashRate));
			}
			else
			{
				crystalAlienMaterial.SetColor("_Color", Color.Lerp(Color.red, Color.white, 2 - flashRate));
			}


		}
	}

	IEnumerator DestroySelfOnDelay() {
		yield return new WaitForSeconds( explosionTimer );
		myHealth.Damage(999999);
		this.enabled = false;
	}
}
