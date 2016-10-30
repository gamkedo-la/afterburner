using UnityEngine;
using System.Collections;

public class TrackWithCamera : MonoBehaviour 
{
	private Camera m_camera;
	

	void Awake()
	{
		m_camera = Camera.main;
	}

	
	void Update() 
	{
		var position = m_camera.transform.position;
		position.y = transform.position.y;

		transform.position = position;
	}
}
