using UnityEngine;
using System.Collections;

public class SpinPropeller : MonoBehaviour
{
    [SerializeField] Vector2 m_spinSpeedMinMax = new Vector2(1000f, 3000f); 

    private FlyingControl m_flyingControlScript;
    private IDamageable m_healthScript;
    private float m_maxForwardSpeed;
    private float m_spinSpeed;
    private bool m_crashed;


    void Awake()
    {
        m_flyingControlScript = GetComponentInParent<FlyingControl>();
        m_healthScript = GetComponentInParent<EnemyHealth>();
        m_maxForwardSpeed = m_flyingControlScript.MaxForwardSpeed;
    }


    void Update()
    {
        if (m_healthScript != null && (m_healthScript.IsCrashedOnGround ||m_healthScript.IsInWater))
            gameObject.SetActive(false);

        if (m_flyingControlScript == null)
        { 
            transform.Rotate(Vector3.forward, m_spinSpeedMinMax.y * Time.deltaTime);
            return;
        }

        float forwardSpeed = m_flyingControlScript.ForwardSpeed;

        m_spinSpeed = Mathf.Lerp(m_spinSpeedMinMax.x, m_spinSpeedMinMax.y, forwardSpeed / m_maxForwardSpeed);

        transform.Rotate(Vector3.forward, m_spinSpeed * Time.deltaTime);
    }
}
