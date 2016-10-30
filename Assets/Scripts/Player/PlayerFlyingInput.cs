using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlyingControl))]
public class PlayerFlyingInput : MonoBehaviour
{
    private static readonly string Vertical = "Vertical";
    private static readonly string Horizontal = "Horizontal";
    private static readonly string Acceleration = "Acceleration";

    private FlyingControl m_flyingControlScript;


    void Awake()
    {
        m_flyingControlScript = GetComponent<FlyingControl>();
    }


	void Update()
    {
        float v = Input.GetAxis(Vertical);
        float h = Input.GetAxis(Horizontal);
        float a = Input.GetAxis(Acceleration);

        m_flyingControlScript.PitchAndRollInput(v, h);
        m_flyingControlScript.ThrustInput(a);
    }
}
