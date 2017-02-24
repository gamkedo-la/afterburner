using UnityEngine;
using System.Collections;

public class GunTurretControl : MonoBehaviour
{
	[SerializeField] protected Transform m_horizontalPivotPoint;
	[SerializeField] Transform m_verticalPivotPoint;
	[SerializeField] float m_turnRate = 40f;
	[SerializeField] float m_pitchRate = 40f;
	[SerializeField] float m_startPitch = 30f;
	[SerializeField] float m_minPitch = 5f;
	[SerializeField] float m_maxPitch = 85f;
	[SerializeField] bool invert;

	private float m_v;
	private float m_h;


	void Start()
	{
		m_verticalPivotPoint.localRotation = Quaternion.Euler(-m_startPitch, 0f, 0f);
	}



	protected virtual void Update()
	{
		m_horizontalPivotPoint.Rotate(Vector3.up, m_h * m_turnRate * Time.deltaTime);

		
		m_verticalPivotPoint.Rotate(Vector3.right, m_v * m_pitchRate * Time.deltaTime);

		var verticalRotation = m_verticalPivotPoint.localEulerAngles;

		verticalRotation.x = Mathf.Clamp(verticalRotation.x, 360f - m_maxPitch, 360f - m_minPitch);

		m_verticalPivotPoint.localEulerAngles = verticalRotation;
	}

	public void Move(float v, float h)
	{
		m_v = invert ? -v : v;
		m_h = invert ? -h : h;
		Debug.DrawLine(m_verticalPivotPoint.position, m_verticalPivotPoint.position + 10 * m_verticalPivotPoint.forward, Color.yellow);

		Debug.DrawLine(m_verticalPivotPoint.position, m_verticalPivotPoint.position + 15 * (Quaternion.AngleAxis(m_v, Vector3.right) * Quaternion.AngleAxis(m_h, Vector3.up) * m_verticalPivotPoint.forward), Color.green);

		m_v = Mathf.Clamp(m_v, -1f, 1f); 
		m_h = Mathf.Clamp(m_h, -1f, 1f);
	}

	public Vector2 getOrientation()
	{
		return new Vector2(m_horizontalPivotPoint.localEulerAngles.y, m_verticalPivotPoint.localEulerAngles.x);
	}

	public float getPitchRate()
	{
		return m_pitchRate;
	}

	public float getTurnRate()
	{
		return m_turnRate;
	}
}
