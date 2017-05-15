using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlyingControl))]
public class PlayerFlyingInput : MonoBehaviour
{
	private static readonly string Vertical = "Vertical";
	private static readonly string Horizontal = "Horizontal";
	private static readonly string Acceleration = "Throttle", AccelerationJoy = "ThrottleJoy";
	private static float lastAccelerationJoy = 0f;
	private static bool AcceleratingWithJoy = false;

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
		lastAccelerationJoy = Input.GetAxis(AccelerationJoy);

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
		}
		else
		{
			v = Input.GetAxis(Vertical);
			h = Input.GetAxis(Horizontal);
		}

		a = getAccelerationInput();

		prevMouseX = Input.mousePosition.x;
		prevMouseY = Input.mousePosition.y;

		//Input is already inverted, so don't change it if controls are set to invert
		v = invert ? v : -v;

		m_flyingControlScript.PitchAndRollInput(v, h);
		m_flyingControlScript.ThrustInput(a);
	}

	private float getAccelerationInput()
	{
		float joyInput = Input.GetAxis(AccelerationJoy);
		float keyboardInput = Input.GetAxis(Acceleration);

		if(!Mathf.Approximately(joyInput, lastAccelerationJoy))
		{
			AcceleratingWithJoy = true;
			lastAccelerationJoy = joyInput;
		}
		if(keyboardInput != 0)
		{
			AcceleratingWithJoy = false;
		}

		if(AcceleratingWithJoy)
		{
			return calculateJoy();
		}
		else
		{
			return keyboardInput;
		}
	}

	private float calculateJoy()
	{
		float normalizedInput = (Input.GetAxis(AccelerationJoy) + 1) / 2;
		float currentSpeedPercent = (m_flyingControlScript.ForwardSpeed - m_flyingControlScript.MinForwardSpeed) / (m_flyingControlScript.MaxForwardSpeed - m_flyingControlScript.MinForwardSpeed);
		float speedRange = m_flyingControlScript.MaxForwardSpeed - m_flyingControlScript.MinForwardSpeed;

		if(Mathf.Approximately(normalizedInput, currentSpeedPercent)) //We have reached desired speed
		{
			AcceleratingWithJoy = false;
			return 0;
		}
		else
		{
			float deltaSpeed = normalizedInput * m_flyingControlScript.AccelerationRate() * Time.deltaTime;
			float nextSpeed = deltaSpeed + m_flyingControlScript.ForwardSpeed;
			float targetSpeed = normalizedInput * speedRange + m_flyingControlScript.MinForwardSpeed;

			Debug.Log("tar , forw: " + targetSpeed + " , " + m_flyingControlScript.ForwardSpeed);

			if(normalizedInput < currentSpeedPercent) //Decelerating
			{
				Debug.Log("de");
				if(nextSpeed < targetSpeed)
				{
					AcceleratingWithJoy = false;
					return 2*(nextSpeed - targetSpeed) / speedRange;
				}
				return -1f;
			}
			else if(normalizedInput > currentSpeedPercent) //Accelerating
			{
				Debug.Log("ac");
				if(nextSpeed > targetSpeed)
				{
					Debug.Log("ac");
					AcceleratingWithJoy = false;
					return 2*(nextSpeed - targetSpeed) / speedRange;
				}
				return 1f;
			}
			return 0;
		}
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
				break;
			case MouseControlsButton.MouseControlSettings.enabled:
				mouseControlActivationThreshold = 0f;
				PlayerPrefs.SetInt("mouse controls", 1);
				break;
			case MouseControlsButton.MouseControlSettings.auto:
				mouseControlActivationThreshold = 2f;
				PlayerPrefs.SetInt("mouse controls", 2);
				break;
		}
  }
}