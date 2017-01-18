using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;

public class GameController : MonoBehaviour
{
    public static bool AllowCheatMode = false;

    [SerializeField] AudioMixerSnapshot m_sfxFullVolume;
    [SerializeField] AudioMixerSnapshot m_sfxMuted;
    [SerializeField] AudioMixerSnapshot m_musicFullVolume;
    [SerializeField] AudioMixerSnapshot m_musicMuted;

    [SerializeField] float m_musicFadeTime = 1f;

    private Camera m_mainCamera;
    private bool m_freeCameraEnabled = false;
    private bool m_paused = false;
    private float m_timeScale;

    private CameraFlyingControl m_freeCameraScript;
    private Vector3 m_lastCameraPosition;
    private Quaternion m_lastCameraRotation;
    private Transform m_cameraParent;


    void Awake()
    {
        OnUnpause();

        m_mainCamera = Camera.main;
        m_cameraParent = m_mainCamera.transform.parent;

        m_timeScale = Time.timeScale;
    }


    void Start()
    {
        if (AllowCheatMode)
        {
            m_freeCameraScript = m_mainCamera.GetComponent<CameraFlyingControl>();

            if (m_freeCameraScript == null)
                m_freeCameraScript = m_mainCamera.gameObject.AddComponent<CameraFlyingControl>();

            m_freeCameraScript.enabled = false;
        }
    }


    void Update()
    {
        if (AllowCheatMode)
        {
            if (Input.GetKeyDown(KeyCode.F))
                ToggleFreeCamera();

            if (m_freeCameraEnabled && Input.GetKeyDown(KeyCode.P))
            {
                m_paused = !m_paused;
                SetTimeScale();
            }
        }
    }


    private void ToggleFreeCamera()
    {
        m_freeCameraEnabled = !m_freeCameraEnabled;
        m_paused = m_freeCameraEnabled;

        if (m_freeCameraEnabled)
        {     
            m_lastCameraPosition = m_mainCamera.transform.localPosition;
            m_lastCameraRotation = m_mainCamera.transform.localRotation;
            m_mainCamera.transform.parent = null;
            var rotation = m_lastCameraRotation.eulerAngles;
            rotation.z = 0;
            m_mainCamera.transform.rotation.eulerAngles.Set(rotation.x, rotation.y, rotation.z);
        }
        else
        {
            m_mainCamera.transform.parent = m_cameraParent;
            m_mainCamera.transform.localPosition = m_lastCameraPosition;
            m_mainCamera.transform.localRotation = m_lastCameraRotation;       
        }

        SetTimeScale();

        m_freeCameraScript.enabled = m_freeCameraEnabled;

        //EventManager.TriggerEvent(EventNames.ToggleFreeCamera, m_freeCameraEnabled);
    }


    private void SetTimeScale()
    {
        Time.timeScale = m_paused ? 0 : m_timeScale;
    }


    public void LoadLevel(string levelToLoad)
    {
        LastMenuUsed.LastMenuUsedName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(levelToLoad);
    }


    public void QuitGame()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void OnPause()
    {
        Time.timeScale = 0;
        //print("Pause");
        if (m_sfxMuted != null)
            m_sfxMuted.TransitionTo(0.01f);
    }


    private void OnUnpause()
    {
        Time.timeScale = 1;
        //print("Unpause");
        if (m_sfxFullVolume != null)
            m_sfxFullVolume.TransitionTo(0.01f);
    }


    private void MusicFadeDown()
    {
        //print("Music fade down");
				
        if (m_musicMuted != null)
            m_musicMuted.TransitionTo(m_musicFadeTime);
    }


    private void MusicFadeUp()
    {
        //print("Music fade up");
				
        if (m_musicFullVolume != null)
            m_musicFullVolume.TransitionTo(m_musicFadeTime);
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.Pause, OnPause);
        EventManager.StartListening(StandardEventName.Unpause, OnUnpause);
        EventManager.StartListening(StandardEventName.ActivateCameraPan, MusicFadeUp);
        EventManager.StartListening(StandardEventName.MissionFailed, MusicFadeUp);
        EventManager.StartListening(StandardEventName.ReturnToMenu, MusicFadeUp);
        EventManager.StartListening(StandardEventName.StartMission, MusicFadeDown);
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.Pause, OnPause);
        EventManager.StopListening(StandardEventName.Unpause, OnUnpause);
        EventManager.StopListening(StandardEventName.ActivateCameraPan, MusicFadeUp);
        EventManager.StopListening(StandardEventName.MissionFailed, MusicFadeUp);
        EventManager.StopListening(StandardEventName.ReturnToMenu, MusicFadeUp);
        EventManager.StopListening(StandardEventName.StartMission, MusicFadeDown);

        Time.timeScale = m_timeScale;
    }
}

