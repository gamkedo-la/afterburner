using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonSelectedOnStart : MonoBehaviour
{
    private Button m_button;


	void Start ()
    {
        m_button = GetComponent<Button>();

        m_button.Select();
	}
}
