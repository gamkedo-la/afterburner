using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ShootingControl))]
public class PlayerShootingInput : MonoBehaviour
{
    private static readonly string Shoot = "Shoot";
    private ShootingControl m_shootingControlScript;

	
    void Awake()
    {
        m_shootingControlScript = GetComponent<ShootingControl>();
    }


    void Update()
    {
        if (Input.GetAxisRaw(Shoot) == 1 && Time.timeScale > 0)
            m_shootingControlScript.Shoot();
    }
}
