using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class TrailRendererMaterialSwitcher : MonoBehaviour
{
    [SerializeField] Material[] m_trailMaterials;
    [SerializeField] float m_cycleTime = 0.1f;

    private TrailRenderer m_trailRenderer;
    private WaitForSeconds m_wait;
    private int m_currentIndex;


	void Awake()
    {
        m_trailRenderer = GetComponent<TrailRenderer>();
        m_wait = new WaitForSeconds(m_cycleTime);
        m_currentIndex = Random.Range(0, m_trailMaterials.Length);
	}
	

    void Start()
    {
        StartCoroutine(RotateMaterials());
    }


    private IEnumerator RotateMaterials()
    {
        while (true)
        {
            m_trailRenderer.material = m_trailMaterials[m_currentIndex];
            m_currentIndex++;
            m_currentIndex = m_currentIndex % m_trailMaterials.Length;

            yield return m_wait;
        }
    }
}
