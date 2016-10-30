using UnityEngine;
using System.Collections;

public class HideUI : MonoBehaviour
{
    private GameObject m_canvases;


    void Awake()
    {
        m_canvases = GameObject.Find("Canvases");
    }


	void Update()
    {
        if (!GameController.AllowCheatMode)
            return;

        if (Input.GetKeyDown(KeyCode.H) && m_canvases != null)
            m_canvases.SetActive(!m_canvases.activeSelf);
	}
}
