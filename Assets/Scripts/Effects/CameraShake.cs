using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
	private Transform m_camera;
	private Quaternion originalCamRot;

	void Awake()
	{
		m_camera = Camera.main.transform;
		originalCamRot = Camera.main.transform.localRotation;
	}


	private void ShakeCamera(float magnitude, float duration)
	{
		StartCoroutine(Shake(magnitude, duration));
	}


	private IEnumerator Shake(float magnitude, float duration)
	{
		float elapsed = 0.0f;

		//Quaternion originalCamRot = Camera.main.transform.localRotation;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			// map value to [-1, 1]
			float z = Random.value * 2.0f - 1.0f;
			float x = Random.value * 2.0f - 1.0f;
			z *= damper;
			x *= damper;


			m_camera.localRotation = originalCamRot;

			m_camera.localRotation *= Quaternion.AngleAxis(z * magnitude, Vector3.forward);
			m_camera.localRotation *= Quaternion.AngleAxis(x * magnitude, Vector3.right);

			yield return null;
		}

		m_camera.localRotation = originalCamRot;
	}


	void OnEnable()
	{
		EventManager.StartListening(TwoFloatsEventName.ShakeCamera, ShakeCamera);
	}


	void OnDisable()
	{
		EventManager.StopListening(TwoFloatsEventName.ShakeCamera, ShakeCamera);
	}
}
