using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSwitcher : MonoBehaviour
{
    private static readonly string CameraAxis = "Camera";

    [SerializeField] Transform[] m_cameraPositions;
    [SerializeField] int[] m_indicesToShowHud = new int[] { 0, 1 };
    [SerializeField] int[] m_indicesToShowRadar = new int[] { 0, 1 };
    [SerializeField] int[] m_indicesToShowTargetSystem = new int[] { 0, 1 };
    [SerializeField] int[] m_indicesToShowHealthMeter = new int[] { 0, 1 };

    private Transform m_cameraTransform;
    private int m_index;
    private bool m_inputAxisInUse; 


    void Awake()
    {
        m_cameraTransform = Camera.main.transform;
    }

	
	void Update()
    {
        int index = m_index;

        if (Time.timeScale == 0 || PlayerHealth.PlayerDead)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && m_cameraPositions.Length > 0)
            index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && m_cameraPositions.Length > 1)
            index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && m_cameraPositions.Length > 2)
            index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && m_cameraPositions.Length > 3)
            index = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5) && m_cameraPositions.Length > 4)
            index = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6) && m_cameraPositions.Length > 5)
            index = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7) && m_cameraPositions.Length > 6)
            index = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8) && m_cameraPositions.Length > 7)
            index = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9) && m_cameraPositions.Length > 8)
            index = 8;
        else if (Input.GetKeyDown(KeyCode.Alpha0) && m_cameraPositions.Length > 9)
            index = 9;

        int increment = (int) Input.GetAxisRaw(CameraAxis);

        if (increment == 0)
            m_inputAxisInUse = false;
        else if (!m_inputAxisInUse)
        {
            m_inputAxisInUse = true;
            index += increment;
            index = (index + m_cameraPositions.Length) % m_cameraPositions.Length;
        }

        if (m_index != index)
        {
            m_index = index;
            var newTransform = m_cameraPositions[m_index];

            m_cameraTransform.position = newTransform.position;
            m_cameraTransform.rotation = newTransform.rotation;
            m_cameraTransform.parent = newTransform;

            EventManager.TriggerEvent(BooleanEventName.ActivateHud, false);
            EventManager.TriggerEvent(BooleanEventName.ActivateRadar, false);
            EventManager.TriggerEvent(BooleanEventName.ActivateTargetSystem, false);
            EventManager.TriggerEvent(BooleanEventName.ActivateHealthMeter, false);

            for (int i = 0; i < m_indicesToShowHud.Length; i++)
            {
                int indexToShow = m_indicesToShowHud[i];

                if (m_index == indexToShow)
                    EventManager.TriggerEvent(BooleanEventName.ActivateHud, true);             
            }

            for (int i = 0; i < m_indicesToShowRadar.Length; i++)
            {
                int indexToShow = m_indicesToShowRadar[i];

                if (m_index == indexToShow)
                    EventManager.TriggerEvent(BooleanEventName.ActivateRadar, true);
            }

            for (int i = 0; i < m_indicesToShowTargetSystem.Length; i++)
            {
                int indexToShow = m_indicesToShowTargetSystem[i];

                if (m_index == indexToShow)
                    EventManager.TriggerEvent(BooleanEventName.ActivateTargetSystem, true);
            }

            for (int i = 0; i < m_indicesToShowHealthMeter.Length; i++)
            {
                int indexToShow = m_indicesToShowHealthMeter[i];

                if (m_index == indexToShow)
                    EventManager.TriggerEvent(BooleanEventName.ActivateHealthMeter, true);
            }
        }   
    }
}
