using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TerrainButton : MonoBehaviour
{
    private Text m_text;


    void Awake()
    {
        m_text = GetComponentInChildren<Text>();
    }


    void Start()
    {
        SetButtonText();
    }


    private void SetButtonText()
    {
        m_text.text = string.Format("Terrain: {0}", QualityController.TerrainDetail);
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.UpdateTerrainDetail, SetButtonText);
        SetButtonText();
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.UpdateTerrainDetail, SetButtonText);
    }
}
