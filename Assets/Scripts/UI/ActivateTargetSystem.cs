using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class ActivateTargetSystem : MonoBehaviour
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
        EventManager.StartListening(BooleanEventName.ActivateTargetSystem, Activate);
    }


    void OnDisable()
    {
        EventManager.StopListening(BooleanEventName.ActivateTargetSystem, Activate);
    }
}
