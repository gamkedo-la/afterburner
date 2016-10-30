using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class ActivateHud : MonoBehaviour
{
    private Canvas m_canvas;


    void Awake()
    {
        m_canvas = GetComponent<Canvas>();
    }


    private void Activate(bool active)
    {
        m_canvas.enabled = active;
    }


	void OnEnable()
    {
        EventManager.StartListening(BooleanEventName.ActivateHud, Activate);
    }


    void OnDisable()
    {
        EventManager.StopListening(BooleanEventName.ActivateHud, Activate);
    }
}
