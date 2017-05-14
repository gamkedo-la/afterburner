using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonSelectManager : MonoBehaviour
{
	private Button m_thisButton;
	private static Button m_prevButton;
	private EventSystem m_eventSystem;
	private AudioSource m_audioSource;

	void Awake()
	{
		m_thisButton = GetComponent<Button>();
		m_eventSystem = FindObjectOfType<EventSystem>();
		m_audioSource = GetComponent<AudioSource>();
		m_prevButton = m_thisButton;
  }

	//If no button is selected during keyboard navigation, use last selected button
	void OnGUI()
	{
		if(m_eventSystem.currentSelectedGameObject == null)
		{
			if(Input.GetAxis("Menu Horizontal") != 0 || Input.GetAxis("Menu Vertical") != 0)
			{
				m_prevButton.Select();
				
				if(m_audioSource != null && Time.timeSinceLevelLoad > 0.1f)
					m_audioSource.Play();
			}
		}
	}

	public void OnButtonSelected()
	{
		//Prevent selecting an already selected button
		if(m_thisButton.gameObject.Equals(m_eventSystem.currentSelectedGameObject))
		{
			return;
		}

		m_thisButton.Select();

		if(m_audioSource != null && Time.timeSinceLevelLoad > 0.1f)
			m_audioSource.Play();
	}

	//When mouse moves away from button, deselect it
	public void OnButtonDeselected()
	{
		m_prevButton = m_thisButton;
    m_eventSystem.SetSelectedGameObject(null);
	}
}
