using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlyingControl))]
public class PlayerFlyingInput : MonoBehaviour
{
	private static readonly string Vertical = "Vertical";
	private static readonly string Horizontal = "Horizontal";
	private static readonly string Acceleration = "Acceleration";

	private FlyingControl m_flyingControlScript;

	private static bool mouseControl;
	private static float mouseControlActivationThreshold;
  private float prevVerticalAxis, prevHorizontalAxis;
	private float prevMouseX, prevMouseY;
	private static bool invert = true, controlsLoaded = false;

	void Awake()
	{
		m_flyingControlScript = GetComponent<FlyingControl>();
		mouseControl = false;
		prevVerticalAxis = 0f;
		prevHorizontalAxis = 0f;
		prevMouseX = Input.mousePosition.x;
		prevMouseY = Input.mousePosition.y;

		if(!controlsLoaded)
		{
			invert = PlayerPrefs.GetInt("invert controls", 1) > 0 ? true : false;
			setMouseControlActivationThreshold((MouseControlsButton.MouseControlSettings)PlayerPrefs.GetInt("mouse controls", 2));
			controlsLoaded = true;
		}
	}
		
	void Update()
	{
		if (prevHorizontalAxis != Input.GetAxis(Vertical) || prevVerticalAxis != Input.GetAxis(Horizontal))
		{
			mouseControl = false;
		}
		else if (prevMouseX >= Input.mousePosition.x + mouseControlActivationThreshold
			    || prevMouseY >= Input.mousePosition.y + mouseControlActivationThreshold
					|| prevMouseX <= Input.mousePosition.x - mouseControlActivationThreshold
					|| prevMouseY <= Input.mousePosition.y - mouseControlActivationThreshold)
		{
			mouseControl = true;
		}
		float v;
		float h;
		float a;

		if (mouseControl)
		{
			float mouseX = Mathf.Clamp(Input.mousePosition.y / Screen.height, 0f, 1f) * 2 - 1;
			float mouseY = Mathf.Clamp(Input.mousePosition.x / Screen.width, 0f, 1f) * 2 - 1;


			float sharpness = 0.5f;

			if (Mathf.Abs(mouseX) <= 0.5f)
			{
				v = (Mathf.Pow(2f * mouseX, 1f / sharpness)) / 2f;
			}
			else
			{
				v = 1f - (Mathf.Pow(2f * (1f - mouseX), 1f / sharpness)) / 2f;
			}

			if (Mathf.Abs(mouseY) <= 0.5f)
			{
				h = (Mathf.Pow(2f * mouseY, 1f / sharpness)) / 2f;
			}
			else
			{
				h = 1f - (Mathf.Pow(2f * (1f - mouseY), 1f / sharpness)) / 2f;
			}

			v = (Mathf.Pow(2f * mouseX, 1f / sharpness)) / 4f;
			h = (Mathf.Pow(2f * mouseY, 1f / sharpness)) / 4f;

			v = Mathf.Pow(mouseX, 2) * Mathf.Sign(mouseX);
			h = Mathf.Pow(mouseY, 2) * Mathf.Sign(mouseY);
			a = Input.GetAxis(Acceleration);
		}
		else
		{
			v = Input.GetAxis(Vertical);
			h = Input.GetAxis(Horizontal);
			a = Input.GetAxis(Acceleration);
		}

		prevMouseX = Input.mousePosition.x;
		prevMouseY = Input.mousePosition.y;

		//Input is already inverted, so don't change it if controls are set to invert
		v = invert ? v : -v;

		m_flyingControlScript.PitchAndRollInput(v, h);
		m_flyingControlScript.ThrustInput(a);
	}

	public void toggleInvert()
	{
		invert = !invert;

		if(invert)
		{
			PlayerPrefs.SetInt("invert controls", 1);
		}
		else
		{
			PlayerPrefs.SetInt("invert controls", 0);
		}
	}

	public bool getInvert()
	{
		return invert;
	}

	public void setMouseControlActivationThreshold(MouseControlsButton.MouseControlSettings state)
	{
		switch(state)
		{
			case MouseControlsButton.MouseControlSettings.disabled:
				mouseControlActivationThreshold = Screen.width + Screen.height + 1;
				mouseControl = false;
				PlayerPrefs.SetInt("mouse controls", 0);
				Debug.Log("Mouse 0: " + mouseControlActivationThreshold);
				break;
			case MouseControlsButton.MouseControlSettings.enabled:
				mouseControlActivationThreshold = 0f;
				PlayerPrefs.SetInt("mouse controls", 1);
				Debug.Log("Mouse 1");
				break;
			case MouseControlsButton.MouseControlSettings.auto:
				mouseControlActivationThreshold = 2f;
				PlayerPrefs.SetInt("mouse controls", 2);
				Debug.Log("Mouse 2");
				break;
		}
  }
}