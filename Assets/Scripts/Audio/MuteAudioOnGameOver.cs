using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MuteAudioOnGameOver : MonoBehaviour
{
    private AudioSource m_audioSource;

	
    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }


    private void MuteAudioSource()
    {
        m_audioSource.Stop();
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.ActivateCameraPan, MuteAudioSource);
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.ActivateCameraPan, MuteAudioSource);
    }
}
