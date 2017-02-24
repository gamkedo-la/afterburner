using UnityEngine;
using System.Collections;

public class CrystalSmoothSlider : MonoBehaviour {
	private Material crystalAlienMaterial;
	public float smoothness;
	public float smoothnessDelta;

	IEnumerator ChangeMaterial(){
		while (true) {
			yield return new WaitForSeconds (0.05f);
			smoothness = Mathf.Repeat((smoothness + 0.05f * smoothnessDelta), 2);
			if(smoothness <= 1)
			{
				crystalAlienMaterial.SetFloat("_Glossiness", smoothness);
			}
			else
			{
				crystalAlienMaterial.SetFloat("_Glossiness", 2 - smoothness);
			}
		}
	}

	void Start () {
		crystalAlienMaterial = gameObject.GetComponent<Renderer>().material;
		//crystalAlienMaterial.SetFloat("_Glossiness", 1f);
		StartCoroutine(ChangeMaterial());
	}
}
