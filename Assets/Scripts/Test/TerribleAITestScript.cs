using UnityEngine;
using System.Collections;

public class TerribleAITestScript : MonoBehaviour {
	public float startDelay;
	public float timer;
	bool whichTimer;
	// Use this for initialization
	void Start () {
		float temp = timer;
		timer = startDelay;
		startDelay = temp;
		whichTimer = false;
	}

	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;

		if(timer < 0 && !whichTimer)
		{
			transform.position = new Vector3(-325.344f, 100, 1163.321f);
			whichTimer = true;
			timer = startDelay;
		}
		else if(timer < 0)
		{
			//output results before quit if making a build
			Debug.Break();
			Application.Quit();
		}
	}
}
