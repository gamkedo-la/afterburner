using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MissionGoals : MonoBehaviour
{
    public static bool MissionSuccessful;

    [SerializeField] bool m_destroyMainTarget = true;
    [SerializeField] bool m_destroyAllGroundDefences = true;
    [SerializeField] bool m_destroyAllAirDefences = true;
    [SerializeField] bool m_destroyAllWaterDefences = true;

    private Transform m_mainTarget;
    private HashSet<Transform> m_groundDefences = new HashSet<Transform>();
    private HashSet<Transform> m_airDefences = new HashSet<Transform>();
    private HashSet<Transform> m_waterDefences = new HashSet<Transform>();

    private bool m_initialised;


    void Awake()
    {
        MissionSuccessful = false;
    }


    public void AddMainTarget(Transform mainTarget)
    {
        m_mainTarget = mainTarget;
    }


    public void AddGroundObject(Transform groundObject)
    {
        m_groundDefences.Add(groundObject);
    }


    public void AddAirObject(Transform airObject)
    {
        m_airDefences.Add(airObject);
    }


    public void AddWaterObject(Transform waterObject)
    {
        m_waterDefences.Add(waterObject);
    }


    private void ObjectDestroyed(Transform transform)
    {
        if (m_mainTarget != null && m_mainTarget == transform)
        {
            m_mainTarget = null;
            //print("Main target destroyed");
        }
        else if (m_groundDefences.Contains(transform))
        {
            m_groundDefences.Remove(transform);
            //print(string.Format("ground object {0} destroyed", transform.name));
        }
        else if (m_airDefences.Contains(transform))
        {
            m_airDefences.Remove(transform);
            //print(string.Format("Air object {0} destroyed", transform.name));
        }
        else if (m_waterDefences.Contains(transform))
        {
            m_waterDefences.Remove(transform);
            //print(string.Format("Water object {0} destroyed", transform.name));
        }

        CheckStatus();
    }


    private void CheckStatus()
    {
        bool success = true;

        if (m_destroyMainTarget)
        {
            success = success && m_mainTarget == null;

            //if (m_mainTarget == null)
            //    print("Main target criterion met");
        }

        if (m_destroyAllGroundDefences)
        {
            success = success && m_groundDefences.Count == 0;

            //if (m_groundDefences.Count == 0)
            //    print("Ground defences criterion met");
        }

        if (m_destroyAllAirDefences)
        {
            success = success && m_airDefences.Count == 0;

            //if (m_airDefences.Count == 0)
            //    print("Air defences criterion met");
        }

        if (m_destroyAllWaterDefences)
        {
            success = success && m_waterDefences.Count == 0;

            //if (m_waterDefences.Count == 0)
            //    print("Water defences criterion met");
        }

        if (success)
        {
            EventManager.TriggerEvent(StandardEventName.MissionSuccessful);
            MissionSuccessful = success;
            print("Mission successful");
        }
    }


    void OnEnable()
    {
        EventManager.StartListening(TransformEventName.EnemyDead, ObjectDestroyed);
    }


    void OnDisable()
    {
        EventManager.StopListening(TransformEventName.EnemyDead, ObjectDestroyed);
    }
}
