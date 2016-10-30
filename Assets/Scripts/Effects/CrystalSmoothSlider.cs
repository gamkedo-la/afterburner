using UnityEngine;
using System.Collections;

public class CrystalSmoothSlider : MonoBehaviour {

	public Material[] smooth;
	Renderer myRenderer;
	int counter = 0;
	int direction = 1;

	IEnumerator ChangeMaterial(){
		while (true) {
			yield return new WaitForSeconds (0.1f);
			counter += direction;
			myRenderer.material = smooth [counter];
			if (counter == 10 || counter == 0) {
				direction *= -1;
			}
		}
	}

	// Use this for initialization
	void Start () {
		myRenderer = gameObject.GetComponent<Renderer> ();
		myRenderer.material = smooth [counter];
		StartCoroutine (ChangeMaterial ());
	}
}
