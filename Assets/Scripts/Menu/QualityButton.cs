using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class QualityButton : MonoBehaviour {

    private Text m_text;
    private QualityController qualityController;


    void Awake()
    {
        m_text = GetComponentInChildren<Text>();
        qualityController = FindObjectOfType<QualityController>();
    }


    void Start()
    {
        SetButtonText();
    }


    private void SetButtonText()
    {
        m_text.text = string.Format("Quality: {0}", qualityController.GetGraphicsQuality());
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.UpdateGraphicsQuality, SetButtonText);
        SetButtonText();
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.UpdateGraphicsQuality, SetButtonText);
    }
}
