using UnityEngine;
using System.Collections;

public class MissionLoader : MonoBehaviour
{
    void Awake()
    {
        if (MissionManager.MissionToLoad != null)
            Instantiate(MissionManager.MissionToLoad.gameObject, Vector3.zero, Quaternion.identity);
    }


    void Start()
    {
        //print("Start mission triggered");
        EventManager.TriggerEvent(StandardEventName.StartMission);
    }
}
