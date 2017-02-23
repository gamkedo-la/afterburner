using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MouseControlsButton : MonoBehaviour
{
	public Color enabledColor, disabledColor, autoColor;
	public Color textEnabledColor, textDisabledColor, textAutoColor;
	private Text m_text;
	private Image m_image;
	MouseControlSettings mouseControls;

	public enum MouseControlSettings { disabled, enabled, auto };

	void Awake()
	{
		m_text = GetComponentInChildren<Text>();
		m_image = GetComponent<Image>();
	}


	void Start()
	{
		mouseControls = (MouseControlSettings)PlayerPrefs.GetInt("mouse controls", 2);
		SetButtonColor();
	}


	private void SetButtonColor()
	{
		switch(mouseControls)
		{
			case MouseControlSettings.disabled:
				m_image.color = disabledColor;
				m_text.color = textDisabledColor;
				m_text.text = "Mouse Disabled";
				break;
			case MouseControlSettings.enabled:
				m_image.color = enabledColor;
				m_text.color = textEnabledColor;
				m_text.text = "Mouse Enabled";
				break;
			case MouseControlSettings.auto:
				m_image.color = autoColor;
				m_text.color = textAutoColor;
				m_text.text = "Mouse Auto";
				break;
		}
	}

	public void clicked()
	{
		//Increment enum
		mouseControls = (MouseControlSettings)(((int)mouseControls + 1) % 3);

		SetButtonColor();

		PlayerFlyingInput flyingInput = FindObjectOfType<PlayerFlyingInput>();
		flyingInput.setMouseControlActivationThreshold(mouseControls);
	}
}
