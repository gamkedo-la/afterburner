using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class MissionManager : MonoBehaviour
{
    public static ProceduralPlacement MissionToLoad;

    [SerializeField] string m_year;
    [SerializeField] Text m_missionNameText;
    [SerializeField] Text m_missionStoryText;
    [SerializeField] Text m_missionGoalText;
    [SerializeField] ProceduralPlacement[] m_missions;
    [SerializeField] Text[] m_buttons;
    [SerializeField] Color m_clickedColour;

    private string m_indexKey;
    private Color m_unclickedColour;


    void Awake()
    {
        if (m_missions.Length == 0)
            return;

        m_indexKey = string.Format("{0} mission selected", m_year);
        int index = PlayerPrefs.GetInt(m_indexKey);

        m_unclickedColour = m_buttons[0].color;

        m_buttons[index].GetComponent<Button>().Select();

        SetMission(index);
    }

	
    public void SetMission(int index)
    {
        PlayerPrefs.SetInt(m_indexKey, index);
        SetButtonColours(index);

        MissionToLoad = m_missions[index];

        if (m_missionNameText != null)
            m_missionNameText.text = GetMissionName(index);

        if (m_missionStoryText != null)
            m_missionStoryText.text = GetMissionStory(index);

        if (m_missionGoalText != null)
            m_missionGoalText.text = GetMissionGoal(index);
    }


    public string GetMissionName(int index)
    {    
        return m_missions[index].missionName;
    }


    public string GetMissionStory(int index)
    {
        return m_missions[index].missionStory;
    }


    public string GetMissionGoal(int index)
    {
        return m_missions[index].missionGoal;
    }


    private void SetButtonColours(int index)
    {
        for (int i = 0; i < m_buttons.Length; i++)
            m_buttons[i].color = m_unclickedColour;

        m_buttons[index].color = m_clickedColour;
    }
}
