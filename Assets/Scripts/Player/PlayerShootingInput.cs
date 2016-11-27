using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class PlayerShootingInput : MonoBehaviour
{
    private static readonly string Shoot = "Shoot";

	private DisplayTarget m_targetInfo;
	private GameObject m_rocketPrefab;
	private FlyingControl m_flyingControlScript;
	private ShootingControl m_shootingControlScript;

	void Start()
	{
		m_shootingControlScript = GetComponent<ShootingControl>();
		m_flyingControlScript = GetComponent<FlyingControl>();

		GameObject targetTrackerGO = (GameObject)GameObject.Find("Target Canvas");
		if(targetTrackerGO) {
			m_targetInfo = targetTrackerGO.GetComponent<DisplayTarget>();
			m_rocketPrefab = (GameObject)Resources.Load("GuidedMissile");
		}
    }


    void Update()
    {
        if (Input.GetAxisRaw(Shoot) == 1 && Time.timeScale > 0)
            m_shootingControlScript.Shoot();

		if(m_targetInfo && Input.GetButtonDown("Rocket")) {
			GameObject rocketGO = (GameObject)GameObject.Instantiate(m_rocketPrefab,
				transform.position + -2.0f * Vector3.up + Vector3.forward * 1.0f, transform.rotation);
			LockOnTarget lotScript = rocketGO.GetComponent<LockOnTarget>();

			if (m_flyingControlScript != null)
				lotScript.rocketSpeed = m_flyingControlScript.ForwardVelocity.magnitude;
			
			lotScript.chaseTarget = m_targetInfo.returnCurrentTarget();
		}
    }
}
