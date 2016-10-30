using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonSelectManager : MonoBehaviour
{
    private Button m_thisButton;
    //private EventSystem m_eventSystem;
    private AudioSource m_audioSource;


	void Awake()
    {
        m_thisButton = GetComponent<Button>();
        //m_eventSystem = FindObjectOfType<EventSystem>();
        m_audioSource = GetComponent<AudioSource>();
    }


    public void OnButtonSelected()
    {
        //m_eventSystem.SetSelectedGameObject(null);
        m_thisButton.Select();

        if (m_audioSource != null && Time.timeSinceLevelLoad > 0.1f)
            m_audioSource.Play();  
    }
}
