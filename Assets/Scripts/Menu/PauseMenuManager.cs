using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Canvas))]
public class PauseMenuManager : MonoBehaviour
{
    private Canvas m_canvasComponent;
    private bool m_inputInUse;
    private EventSystem m_eventSystem;


    void Awake()
    {
        m_canvasComponent = GetComponent<Canvas>();
        m_canvasComponent.enabled = false;
        m_eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
        m_eventSystem.sendNavigationEvents = false;
    }


    void Update()
    {
        if (Input.GetAxisRaw("Pause") == 1
            && !PlayerHealth.PlayerDead
            && !MissionGoals.MissionSuccessful)
        {
            if (!m_inputInUse)
            {
                m_inputInUse = true;

                if (Time.timeScale != 0)
                {
                    EventManager.TriggerEvent(StandardEventName.Pause);
                    m_canvasComponent.enabled = true;
                    m_eventSystem.sendNavigationEvents = true;
                }
                else
                {
                    EventManager.TriggerEvent(StandardEventName.Unpause);
                    m_canvasComponent.enabled = false;
                    m_eventSystem.sendNavigationEvents = false;
                }
            }
        }
        
        if (Input.GetAxisRaw("Pause") != 1)
            m_inputInUse = false;
    }


    public void LoadLevel(string levelToLoad)
    {
        EventManager.TriggerEvent(StandardEventName.Unpause);
        print("Return to menu triggered");
        EventManager.TriggerEvent(StandardEventName.ReturnToMenu);
        SceneManager.LoadScene(levelToLoad);
    }


    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    public void SetGraphicsQuality()
    {
        EventManager.TriggerEvent(StandardEventName.SetGraphicsQuality);
    }


    public void SetTerrainDetail()
    {
        EventManager.TriggerEvent(StandardEventName.SetTerrainDetail);
    }
}
