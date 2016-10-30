using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class EnemyShootingAiInput : MonoBehaviour
{
    [SerializeField] float m_minDotProductForShooting = 0.7f;
    [SerializeField] float m_minRangeForShooting = 200f;

    private ShootingControl m_shootingControlScript;
    private Transform m_player;


    void Awake()
    {
        m_shootingControlScript = GetComponent<ShootingControl>();
        var playerObject = GameObject.FindGameObjectWithTag(Tags.Player);

        if (playerObject != null)
            m_player = playerObject.transform;
    }


    void Update()
    {
        if (m_player == null || PlayerHealth.PlayerDead)
            return;

        if (IsPlayerInRange())
            m_shootingControlScript.Shoot();
	}


    private bool IsPlayerInRange()
    {
        var direction = (m_player.position - transform.position);

        float range = direction.magnitude;
        float dot = Vector3.Dot(direction.normalized, transform.forward);

        bool isInFront = dot >= m_minDotProductForShooting;
        bool closeEnough = range <= m_minRangeForShooting;

        return isInFront && closeEnough;
    }
}
