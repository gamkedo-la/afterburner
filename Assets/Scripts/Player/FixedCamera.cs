using UnityEngine;
using System.Collections;

public class FixedCamera : MonoBehaviour
{
    private Vector3 m_relativePosition;
    private Quaternion m_rotation;
    private Transform m_parent;


    void Awake()
    {
        m_parent = transform.parent;
        m_relativePosition = transform.position - m_parent.position;
        m_rotation = transform.rotation;
    }

    
	void LateUpdate()
    {
        transform.position = m_parent.position + m_relativePosition;
        transform.rotation = m_rotation;
	}
}
