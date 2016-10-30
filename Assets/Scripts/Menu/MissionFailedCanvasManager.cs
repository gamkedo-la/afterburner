using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class MissionFailedCanvasManager : MonoBehaviour
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
        StartCoroutine(EnableCanvasDelayed());
    }


    private IEnumerator EnableCanvasDelayed()
    {
        yield return new WaitForSeconds(m_delay);
        m_canvas.enabled = true;
        m_eventSystem.sendNavigationEvents = true;
        m_buttonSelectedOnEnable.Select();
    } 


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.MissionFailed, EnableCanvas);
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.MissionFailed, EnableCanvas);
    }
}
