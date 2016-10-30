using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DisplayFPS : MonoBehaviour 
{
	private float m_time;
	private int m_frames;
	private Text m_text;


	void Awake () 
	{
		m_text = GetComponent<Text> ();
	}


	void Update () 
	{
        //if (!GameController.AllowCheatMode)
        //{
        //    fpsText.text = "";
        //    return;
        //}

		m_time += Time.unscaledDeltaTime;
		m_frames++;
		
		if (m_time > 1f)
		{
			m_text.text = string.Format("FPS: {0}", m_frames);
			
			m_time = 0;
			m_frames = 0;
		}
	}
}
