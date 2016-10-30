using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Slider))]
public class ThrustSliderManager : MonoBehaviour
{
    [SerializeField] Slider m_thrustSlider;


	private void SetThrustLevel(float thrust)
    {
        //print("Set thrust to " + thrust);
        if (m_thrustSlider != null)
            m_thrustSlider.value = thrust;
    }


    private void SetMinThrustLevel(float minThrust)
    {
        //print("Set min thrust to " + minThrust);
        if (m_thrustSlider != null)
            m_thrustSlider.minValue = minThrust;
    }


    private void SetMaxThrustLevel(float maxThrust)
    {
        //print("Set max thrust to " + maxThrust);
        if (m_thrustSlider != null)
            m_thrustSlider.maxValue = maxThrust;
    }


    void OnEnable()
    {
        EventManager.StartListening(FloatEventName.SetMinThrustLevel, SetMinThrustLevel);
        EventManager.StartListening(FloatEventName.SetMaxThrustLevel, SetMaxThrustLevel);
        EventManager.StartListening(FloatEventName.SetThrustLevel, SetThrustLevel);
    }


    void OnDisable()
    {
        EventManager.StopListening(FloatEventName.SetMinThrustLevel, SetMinThrustLevel);
        EventManager.StopListening(FloatEventName.SetMaxThrustLevel, SetMaxThrustLevel);
        EventManager.StopListening(FloatEventName.SetThrustLevel, SetThrustLevel);
    }
}
