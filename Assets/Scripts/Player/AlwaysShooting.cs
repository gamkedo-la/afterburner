using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class AlwaysShooting : MonoBehaviour
{
    private ShootingControl m_shootingControlScript;


    void Awake()
    {
        m_shootingControlScript = GetComponent<ShootingControl>();
    }


    void Update()
    {
        m_shootingControlScript.Shoot();
    }
}
