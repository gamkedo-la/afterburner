﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlyingControl))]
public class PlayerFlyingInput : MonoBehaviour
{
	private static readonly string Vertical = "Vertical";
	private static readonly string Horizontal = "Horizontal";
	private static readonly string Acceleration = "Acceleration";

	private FlyingControl m_flyingControlScript;

	private bool mouseControl;
	private float prevVerticalAxis, prevHorizontalAxis;
	private float prevMouseX, prevMouseY;
	private bool invert;

	void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		m_flyingControlScript = GetComponent<FlyingControl>();
		mouseControl = false;
		prevVerticalAxis = 0f;
		prevHorizontalAxis = 0f;
		prevMouseX = 0f;
		prevMouseY = 0f;
		invert = true;
	}


	void Update()
	{
		if (prevHorizontalAxis != Input.GetAxis(Vertical) || prevVerticalAxis != Input.GetAxis(Horizontal))
		{
			mouseControl = false;
		}
		else if (prevMouseX > Input.mousePosition.x + 1 || prevMouseY > Input.mousePosition.y + 1
			    || prevMouseX < Input.mousePosition.x - 1 || prevMouseY < Input.mousePosition.y - 1)
		{
			mouseControl = true;
		}
		float v;
		float h;
		float a;

		if (mouseControl)
		//if (false)
		{
			/*
			Vector2 mouseVector = new Vector2(Mathf.Clamp(Input.mousePosition.y / Screen.height, 0f, 1f) * 2 - 1,
	Mathf.Clamp(Input.mousePosition.x / Screen.width, 0f, 1f) * 2 - 1);
			if (mouseVector.magnitude > 1)
			{
				mouseVector.Normalize();
			}
			*/

			float mouseX = Mathf.Clamp(Input.GetAxis("Mouse Y")*8.0f, -1f, 1f);
			float mouseY = Mathf.Clamp(Input.GetAxis("Mouse X")*8.0f, -1f, 1f);


			float sharpness = 0.5f;

			if (Mathf.Abs(mouseX) <= 0.5f)
			{
				v = (Mathf.Pow(2f * mouseX, 1f / sharpness)) / 2f;
				//v = Mathf.Pow(mouseX, 2) * Mathf.Sign(mouseX);
			}
			else
			{
				v = 1f - (Mathf.Pow(2f * (1f - mouseX), 1f / sharpness)) / 2f;
				//v = 1f - Mathf.Pow(mouseX, 2) * Mathf.Sign(mouseX);
			}

			if (Mathf.Abs(mouseY) <= 0.5f)
			{
				h = (Mathf.Pow(2f * mouseY, 1f / sharpness)) / 2f;
				//v = Mathf.Pow(mouseX, 2) * Mathf.Sign(mouseX);
			}
			else
			{
				h = 1f - (Mathf.Pow(2f * (1f - mouseY), 1f / sharpness)) / 2f;
				//v = 1f - Mathf.Pow(mouseX, 2) * Mathf.Sign(mouseX);
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
	}

	public bool getInvert()
	{
		return invert;
	}
}