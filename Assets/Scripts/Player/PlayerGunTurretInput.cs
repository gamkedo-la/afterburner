using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GunTurretControl))]
public class PlayerGunTurretInput : MonoBehaviour
{
    private GunTurretControl m_turretControlScript;


    void Awake()
    {
        m_turretControlScript = GetComponent<GunTurretControl>();
    }


    void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        m_turretControlScript.Move(v, h);
    }
}
