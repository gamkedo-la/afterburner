using UnityEngine;
using System.Collections;

public class GameOverCameraController : MonoBehaviour
{
    [SerializeField] float m_deathCameraPanSpeed = 20f;
    [SerializeField] float m_cameraDriftSpeed = 10f;
    [SerializeField] float m_maxDistanceFromAnchor = 500f;

    private bool m_dead;
    private bool m_missionSuccessful;

    private Transform m_player;
    private Transform m_camera;


    void Awake()
    {
        m_player = GameObject.FindGameObjectWithTag(Tags.Player).transform;
        m_camera = Camera.main.transform;
    }


    void Update()
    {
        if (m_dead || m_missionSuccessful)
        {
            transform.position = m_player.position;

            transform.Rotate(Vector3.up, m_deathCameraPanSpeed * Time.unscaledDeltaTime, Space.World);

            float distanceFromAnchor = (m_camera.position - transform.position).magnitude;

            if (distanceFromAnchor < m_maxDistanceFromAnchor)
            {
                float distanceToMove = Mathf.Min(m_maxDistanceFromAnchor - distanceFromAnchor, 
                    m_cameraDriftSpeed * Time.unscaledDeltaTime);

                m_camera.Translate(-Vector3.forward * distanceToMove);
            }
        }
    }


    private void PlayerDead(string colliderTag)
    {
        m_dead = true;
        transform.rotation = Quaternion.identity;

        DetachCamera();
        
        EventManager.TriggerEvent(StandardEventName.MissionFailed);
        //print("Mission failed");
    }


    private void MissionSuccessful()
    {
        transform.rotation = Quaternion.identity;
        Time.timeScale = 0;
        m_missionSuccessful = true;
        DetachCamera();
    }


    private void DetachCamera()
    {
        var cameraPosition = transform.GetChild(0);

        Camera.main.transform.parent = cameraPosition;
        Camera.main.transform.position = cameraPosition.position;
        Camera.main.transform.rotation = cameraPosition.rotation;

        transform.parent = null;

        EventManager.TriggerEvent(BooleanEventName.ActivateHud, false);
        EventManager.TriggerEvent(BooleanEventName.ActivateRadar, false);
        EventManager.TriggerEvent(BooleanEventName.ActivateTargetSystem, false);
        EventManager.TriggerEvent(BooleanEventName.ActivateHealthMeter, false);
    }


    void OnEnable()
    {
        EventManager.StartListening(StringEventName.PlayerDead, PlayerDead);
        EventManager.StartListening(StandardEventName.ActivateCameraPan, MissionSuccessful);
    }


    void OnDisable()
    {
        EventManager.StopListening(StringEventName.PlayerDead, PlayerDead);
        EventManager.StopListening(StandardEventName.ActivateCameraPan, MissionSuccessful);
    }
}
