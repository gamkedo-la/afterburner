using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour 
{
	public static Vector3 windSpeed;
	private static WindZone windZone;

	public float windSpeedToForceScaler = 0.2f;
	public float minWindSpeed = 1f;
	public float maxWindSpeed = 5f;
	public float windRotationSpeed = 1f;

	private float m_windSpeedMag;
	private float m_windDirectionDeg;


	void Awake()
	{
		if (windZone == null)
			windZone = FindObjectOfType(typeof(WindZone)) as WindZone;
		
		if (windSpeed == Vector3.zero)
		{
			m_windSpeedMag = Random.Range(minWindSpeed, maxWindSpeed);
			m_windDirectionDeg = Random.Range(0, 360f);
			
			SetWindSpeed(m_windSpeedMag, m_windDirectionDeg);
		}
	}
	

	private void SetWindSpeed(float windSpeedMag, float windDirectionDeg)
	{
		m_windSpeedMag = windSpeedMag;
		m_windDirectionDeg = windDirectionDeg;

		float windDirectionRad = Mathf.Deg2Rad * windDirectionDeg;
		
		float windX = windSpeedMag * Mathf.Sin(windDirectionRad);
		float windZ = windSpeedMag * Mathf.Cos(windDirectionRad);
		windSpeed.Set(windX, 0, windZ);
		
		if (windZone != null)
		{
			windZone.transform.rotation = Quaternion.AngleAxis(windDirectionDeg, Vector3.up);
			windZone.windMain = windSpeedToForceScaler * windSpeedMag;
		}
	}


	void Update()
	{
		m_windDirectionDeg += Time.deltaTime * windRotationSpeed;

		SetWindSpeed(m_windSpeedMag, m_windDirectionDeg);
	}
}
