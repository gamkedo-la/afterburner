using UnityEngine;
using System.Collections;

public class EarthPosition : MonoBehaviour
{
    private Transform m_camera;
    private Vector3 m_offset;


	void Start()
    {
        m_camera = Camera.main.transform;
        m_offset = transform.position - m_camera.position;

        transform.rotation = Quaternion.LookRotation(m_offset);
    }
	

	void Update()
    {
        transform.position = m_camera.position + m_offset;
    }
}
