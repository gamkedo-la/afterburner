using UnityEngine;
using System.Collections;

public class GunTurretControl : MonoBehaviour
{
	[SerializeField]
	Transform m_horizontalPivotPoint;
	[SerializeField]
	Transform m_verticalPivotPoint;
	[SerializeField]
	float m_turnRate = 40f;
	[SerializeField]
	float m_pitchRate = 40f;
	[SerializeField]
	float m_startPitch = 30f;
	[SerializeField]
	float m_minPitch = 5f;
	[SerializeField]
	float m_maxPitch = 85f;

	private float m_v;
	private float m_h;


	void Start()
	{
		m_verticalPivotPoint.localRotation = Quaternion.Euler(-m_startPitch, 0f, 0f);
	}


	void Update()
	{
		m_horizontalPivotPoint.Rotate(Vector3.up, m_h * Time.deltaTime);
		m_verticalPivotPoint.Rotate(Vector3.right, m_v * Time.deltaTime);

		var verticalRotation = m_verticalPivotPoint.localEulerAngles;

		verticalRotation.x = Mathf.Clamp(verticalRotation.x, 360f - m_maxPitch, 360f - m_minPitch);

		m_verticalPivotPoint.localEulerAngles = verticalRotation;
	}


	public void Move(float v, float h)
	{
		m_v = v;
		m_h = h;

		m_v = Mathf.Clamp(m_h, -m_pitchRate, m_pitchRate); 
		m_h = Mathf.Clamp(m_v, -m_turnRate, m_turnRate);
	}
}
