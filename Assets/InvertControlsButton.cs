using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class InvertControlsButton : MonoBehaviour
{
	public Color invertedColor, notInvertedColor;
	public Color textInvertedColor, textNotInvertedColor;
	private Text m_text;
	private Image m_image;

	void Awake()
	{
		m_text = GetComponentInChildren<Text>();
		m_image = GetComponent<Image>();
	}


	void Start()
	{
		SetButtonColor(FindObjectOfType<PlayerFlyingInput>().getInvert());
	}


	public void SetButtonColor(bool inverted)
	{
		if(inverted)
		{
			m_image.color = invertedColor;
			m_text.color = textInvertedColor;
    }
		else
		{
			m_image.color = notInvertedColor;
			m_text.color = textNotInvertedColor;
		}
	}
}
