using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(PlaceableObjectGround))]
public class RigidbodyManager : MonoBehaviour
{
    [SerializeField] float m_distanceThreshold = 350f;
    [SerializeField] float m_settlingTime = 2f;

    private Rigidbody m_rigidbody;
    private Transform m_camera;

    private float m_distSq;
    private bool m_settled;

    
    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_camera = Camera.main.transform;
        m_distSq = m_distanceThreshold * m_distanceThreshold;
    }


    void Start()
    {
        m_rigidbody.isKinematic = true;
    }


	void Update()
    {
        if (m_settled)
            return;

        var pos2 = new Vector2(transform.position.x, transform.position.z);
        var cameraPos2 = new Vector2(m_camera.position.x, m_camera.position.z);

        float distSq = (pos2 - cameraPos2).sqrMagnitude;

        if (distSq < m_distSq)
            StartCoroutine(Settle());
	}


    private IEnumerator Settle()
    {
        m_settled = true;
        m_rigidbody.isKinematic = false;

        yield return new WaitForSeconds(m_settlingTime);
     
        m_rigidbody.isKinematic = true;

        var colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].isTrigger = true;
    } 
}
