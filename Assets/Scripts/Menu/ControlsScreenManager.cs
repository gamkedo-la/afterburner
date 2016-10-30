using UnityEngine;
using System.Collections;

public class ControlsScreenManager : MonoBehaviour
{
    [SerializeField] GameObject m_keyboardControls;
    [SerializeField] GameObject m_joypadControls;


    void Awake()
    {
        ShowKeyboardControls();
    }

	
    public void ShowKeyboardControls()
    {
        m_keyboardControls.SetActive(true);
        m_joypadControls.SetActive(false);
    }


    public void ShowJoypadControls()
    {
        m_keyboardControls.SetActive(false);
        m_joypadControls.SetActive(true);
    }
}
