using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class MissionCompleteCanvasManager : MonoBehaviour
{
    [SerializeField] float m_delay = 3f;
    [SerializeField] Button m_buttonSelectedOnEnable;

    private Canvas m_canvas;
    private EventSystem m_eventSystem;


    void Awake()
    {
        m_canvas = GetComponent<Canvas>();
        m_canvas.enabled = false;
        m_eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }


    private void EnableCanvas()
    {
        if (!PlayerHealth.PlayerDead)
            StartCoroutine(EnableCanvasDelayed());
    }


    private IEnumerator EnableCanvasDelayed()
    {
        yield return new WaitForSeconds(m_delay);

        if (!PlayerHealth.PlayerDead)
        {
            m_canvas.enabled = true;
            m_eventSystem.sendNavigationEvents = true;
            m_buttonSelectedOnEnable.Select();
            EventManager.TriggerEvent(StandardEventName.ActivateCameraPan);
        }
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.MissionSuccessful, EnableCanvas);
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.MissionSuccessful, EnableCanvas);
    }
}
