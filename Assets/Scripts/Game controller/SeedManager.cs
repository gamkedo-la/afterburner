using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SeedManager : MonoBehaviour
{
    public static string TerrainSeedString;
    public static int TerrainSeed;
    public static string MissionSeedString;
    public static int MissionSeed;

    [SerializeField] string m_year;
    [SerializeField] InputField m_terrainSeedText;
    [SerializeField] InputField m_missionSeedText;

    private string m_terrainSeedStringKey;
    private string m_missionSeedStringKey;


    void Awake()
    {
        m_terrainSeedStringKey = string.Format("Terrain seed string - {0}", m_year);
        m_missionSeedStringKey = string.Format("Mission seed string - {0}", m_year);
    }


    void Start()
    {
        TerrainSeedString = PlayerPrefs.GetString(m_terrainSeedStringKey);
        MissionSeedString = PlayerPrefs.GetString(m_missionSeedStringKey);

        if (string.IsNullOrEmpty(TerrainSeedString))
            TerrainSeedString = "0";

        if (string.IsNullOrEmpty(MissionSeedString))
            MissionSeedString = "0";

        if (TerrainSeedString == null)
        {
            TerrainSeed = TerrainSeedString.GetHashCode();
            TerrainSeedString = TerrainSeed.ToString();
        }

        if (MissionSeedString == null)
        {
            MissionSeed = MissionSeedString.GetHashCode();
            MissionSeedString = MissionSeed.ToString();
        }

        if (m_terrainSeedText != null)
            m_terrainSeedText.text = TerrainSeedString;

        if (m_missionSeedText != null)
            m_missionSeedText.text = MissionSeedString;
    }


    public void SetTerrainSeed(string input)
    {
        int seed = 0;

        if (!int.TryParse(input, out seed))
            seed = input.GetHashCode();

        TerrainSeed = seed;
        TerrainSeedString = input;

        PlayerPrefs.SetString(m_terrainSeedStringKey, input);
    }


    public void SetMissionSeed(string input)
    {
        int seed = 0;

        if (!int.TryParse(input, out seed))
            seed = input.GetHashCode();

        MissionSeed = seed;
        MissionSeedString = input;

        PlayerPrefs.SetString(m_missionSeedStringKey, input);
    }
}
