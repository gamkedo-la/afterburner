using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum TerrainDetail
{
    High = 0,
    Medium = 1,
    Low = 2,
    SuperLow = 3,
}


public class QualityController : MonoBehaviour
{
    private enum QualityMode
    {
        Automatic,
        Manual
    }


    [Header("Automatic assessment parameters")]
    [SerializeField] float m_assessmentTime = 3f;
    [SerializeField] float m_settlingTime = 2f;
	[SerializeField] float m_maxFrameTimeLimit = 0.1f;
	//[SerializeField] int m_maxFrameTimeFramesToAverage = 4;
    [SerializeField] int m_lowFpsLimit = 30;
    [SerializeField] int m_highFpsLimit = 57;
	[SerializeField] bool m_allowQualityIncrease = false;

    [Header("Options")]
    [SerializeField] QualityMode m_qualityMode = QualityMode.Manual;
    //[SerializeField] Toggle m_autoUpdateToggle;
	//[SerializeField] Text m_maxFrameTimeText;

    public static TerrainDetail TerrainDetail;

    [SerializeField] TerrainDetail m_terrainDetail = TerrainDetail.High;
    private int m_qualityIndex = 0;
    private string[] m_qualityNames;

    private float m_time;
    private int m_frames;
	private float m_frameTimeSum;
	private float m_maxFrameTime;
	private int m_maxframeTimeFrames;
	private bool m_lastDecreaseWasTerrain;
	private bool m_lastIncreaseWasTerrain;
    private string m_terrainPrefs = "Terrain detail";


    void Awake()
    {
        //m_terrainDetail = TerrainDetail.SuperLow;// (TerrainDetail) PlayerPrefs.GetInt(m_terrainPrefs, (int) TerrainDetail.Medium);
        TerrainDetail = m_terrainDetail;
        //print("Terrain detail loaded: " + m_terrainDetail);
        m_qualityNames = QualitySettings.names;
        m_qualityIndex = QualitySettings.GetQualityLevel();
    }


    void Start()
    {
        UpdateGraphicsQuality();
        m_time = -m_assessmentTime * 4f;

        //if (m_autoUpdateToggle != null)
        //	m_autoUpdateToggle.isOn = m_qualityMode == QualityMode.Automatic;
    }


	void Update()
    {
        if (m_qualityMode == QualityMode.Manual)
        {
            //CheckForTerrainDetailInput();
            CheckForGraphicsQualityInput();
            return;
        }

        MonitorFps();
    }


    private void MonitorFps()
    {
        m_time += Time.unscaledDeltaTime;

        if (m_time >= 0)
            m_frames++;

		if (m_time > 0 && Time.unscaledDeltaTime > m_maxFrameTime)
			m_maxFrameTime = Time.unscaledDeltaTime;

		//if (m_maxFrameTimeText != null)
		//	m_maxFrameTimeText.text = string.Format("Max: {0:0} ms", m_maxFrameTime * 1000);

        if (m_time > m_assessmentTime)
        {
            int fps = Mathf.RoundToInt(m_frames / m_time);
            
			if (m_maxFrameTime > m_maxFrameTimeLimit)
				print("Max frame time exceded");

            if (fps < m_lowFpsLimit || m_maxFrameTime > m_maxFrameTimeLimit)
                DecreaseQuality();
            else if (m_allowQualityIncrease && fps > m_highFpsLimit)
                IncreaseQuality();

			m_time = -m_settlingTime;
			m_frames = 0;
			m_maxFrameTime = 0;
        }
    }


    private void DecreaseQuality()
	{
		if (m_lastDecreaseWasTerrain)
		{
			print(Time.time + ": Decreasing graphics quality");
			if (m_qualityIndex > 0)
			{
				m_qualityIndex--;
				m_qualityIndex = Math.Max(m_qualityIndex, 0);
				UpdateGraphicsQuality();
			}
		}
		else
		{
			print(Time.time + ": Decreasing terrain detail");
			if (m_terrainDetail < TerrainDetail.SuperLow)
			{
				m_terrainDetail++;
				m_terrainDetail = (TerrainDetail) Math.Min((int) m_terrainDetail, (int) TerrainDetail.SuperLow);
				UpdateTerrainDetail();
			}
		}

		m_lastDecreaseWasTerrain = !m_lastDecreaseWasTerrain;
	}


    private void IncreaseQuality()
	{
		if (m_lastIncreaseWasTerrain)
		{
			print(Time.time + ": Increasing graphics quality");
			if (m_qualityIndex < m_qualityNames.Length - 1)
			{
				m_qualityIndex++;
				m_qualityIndex = Math.Min(m_qualityIndex, m_qualityNames.Length - 1);
				UpdateGraphicsQuality();
			}
		}
		else
		{
			print(Time.time + ": Increasing terrain detail");
			if (m_terrainDetail > TerrainDetail.High)
			{
				m_terrainDetail--;
				m_terrainDetail = (TerrainDetail) Math.Max((int) m_terrainDetail, (int) TerrainDetail.High);
				UpdateTerrainDetail();
			}
		}

		m_lastIncreaseWasTerrain = !m_lastIncreaseWasTerrain;
	}


    private void CheckForTerrainDetailInput()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
            m_terrainDetail--;

        if (Input.GetKeyDown(KeyCode.Minus))
            m_terrainDetail++;

        UpdateTerrainDetail();
    }


    private void UpdateTerrainDetail()
    {
        m_terrainDetail = (TerrainDetail) (((int) m_terrainDetail + ((int) TerrainDetail.SuperLow + 1))
            % ((int) TerrainDetail.SuperLow + 1));

        if (m_terrainDetail != TerrainDetail)
        {
            TerrainDetail = m_terrainDetail;
            PlayerPrefs.SetInt(m_terrainPrefs, (int) m_terrainDetail);
            print("Terrain detail saved: " + m_terrainDetail);
            EventManager.TriggerEvent(StandardEventName.UpdateTerrainDetail);
        }
    }


    private void CheckForGraphicsQualityInput()
    {
        if (Input.GetKeyDown(KeyCode.Period))
        {
            m_qualityIndex++;
            UpdateGraphicsQuality();
        }

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            m_qualityIndex--;
            UpdateGraphicsQuality();
        }
    }


    private void UpdateGraphicsQuality()
    {
        m_qualityIndex = (m_qualityIndex + m_qualityNames.Length) % m_qualityNames.Length;

        QualitySettings.SetQualityLevel(m_qualityIndex, false);

        EventManager.TriggerEvent(StandardEventName.UpdateGraphicsQuality);
    }


    public void SetAutoUpdateMode(bool autoUpdate)
    {
        //print("Auto-update: " + autoUpdate);
        if (autoUpdate)
            m_qualityMode = QualityMode.Automatic;
        else
            m_qualityMode = QualityMode.Manual;
    }


    public void SetGraphicsQuality()
    {
        m_qualityIndex++;
        UpdateGraphicsQuality();
    }


    public string GetGraphicsQuality()
    {
        if (m_qualityNames != null)
            return m_qualityNames[m_qualityIndex];

        return "";
    }


    public void SetTerrainDetail()
    {
        m_terrainDetail--;
        UpdateTerrainDetail();
    }


    public string GetTerrainDetail()
    {
        return m_qualityNames[m_qualityIndex];
    }


    void OnEnable()
    {
        EventManager.StartListening(StandardEventName.SetGraphicsQuality, SetGraphicsQuality);
        EventManager.StartListening(StandardEventName.SetTerrainDetail, SetTerrainDetail);
    }


    void OnDisable()
    {
        EventManager.StopListening(StandardEventName.SetGraphicsQuality, SetGraphicsQuality);
        EventManager.StopListening(StandardEventName.SetTerrainDetail, SetTerrainDetail);
    }
}
