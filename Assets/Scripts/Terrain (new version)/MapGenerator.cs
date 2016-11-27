using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode
	{
		NoiseMap,
		ColourMap,
		Mesh
	};

	public float uniformScale = 2f;
	//public const int MapChunkSize = 129;//241;

	[SerializeField]
	bool m_useFlatShading;
	[SerializeField]
	bool m_useGlobalSeed = true;
	[SerializeField]
	int m_seed = 0;
	[SerializeField]
	TerrainEquationBase m_terrainEquation;
	[SerializeField]
	DrawMode m_drawMode;

	[Range(0, 6)]
	[SerializeField]
	int m_editorPreviewLod = 0;

	[SerializeField]
	float m_meshHeightMultiplier = 10f;
	[SerializeField]
	AnimationCurve m_meshHeightCurve;

	public bool autoUpdate = true;

	[SerializeField]
	TerrainType[] regions;

	private Queue<MapThreadInfo<MapData>> m_mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	private Queue<MapThreadInfo<MeshData>> m_meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	private Coroutine m_mapDataCoroutine;
	private Coroutine m_meshDataCoroutine;

	private bool m_initialised = false;

	private static MapGenerator Instance;


	public bool UseFlatShading { get { return m_useFlatShading; } }


	public static int MapChunkSize
	{
		get
		{
			if (Instance == null)
				Instance = FindObjectOfType<MapGenerator>();

			return Instance.m_useFlatShading ? 33 : 129;
		}
	}

	public float GetTerrainHeight(float x, float z)
	{
		x /= uniformScale;
		z /= uniformScale;

		x -= 0.5f;    // Offsets needed for some reason I forget, but they're important so don't mess with this!
		z -= 0.5f;

		float height = m_terrainEquation.GetHeight(x, z);

		//print(height);
		height = m_meshHeightCurve.Evaluate(height);
		//print(height);
		height *= m_meshHeightMultiplier;
		//print(height);
		height *= uniformScale;
		//print(height);

		return height;
	}

	public float getRealHeight(float x, float z)
	{
		float scale = uniformScale * 2; //uniformScale must be muliplied by 2 or everything breaks
		float inverseScale = 1 / scale; //for optimization (division is slow)

		//Coordinates of the sides of the square
    float closeX = Mathf.Floor(x * inverseScale) * scale;
		float closeZ = Mathf.Floor(z * inverseScale) * scale;
		float farX = (Mathf.Floor(x * inverseScale) + 1) * scale;
		float farZ = (Mathf.Floor(z * inverseScale) + 1) * scale;

		float intraTileX = x % scale;
		float intraTileZ = z % scale;

		//Corrects for negative values of x and z
		intraTileX = intraTileX >= 0 ? intraTileX : scale + intraTileX;
		intraTileZ = intraTileZ >= 0 ? intraTileZ : scale + intraTileZ;

		//is the position inside the tile above the x = z dividing line?
		bool aboveMidLine = intraTileX == 0 ? true : (intraTileZ / intraTileX > 1 ? true : false);

		// y map:
		// ^   1 _____ 2
		// |     |  /|
		//+Z     | / |
		// |   0 |/__| 3
		// |
		// +--+X--->

		float y0 = GetTerrainHeight(closeX, closeZ);
		float y2 = GetTerrainHeight(farX, farZ);

		if (aboveMidLine)
		{
			float y1 = GetTerrainHeight(closeX, farZ);
			Vector2 intraTilePoint = new Vector2(intraTileX, intraTileZ);

			//lerps the appropiate distance along the leg of the triangle parallel to the x-axis
			float q = Mathf.Lerp(y1, y2, intraTilePoint.x / intraTilePoint.y);
			return Mathf.Lerp(y0, q, intraTileZ * inverseScale); //lerps along the line through the point. scaled to y
		}
		else
		{
			float y3 = GetTerrainHeight(farX, closeZ);
			Vector2 negativeIntraTilePoint = new Vector2(scale - intraTileX, scale - intraTileZ);

			//lerps the appropiate distance along the leg of the triangle parallel to the x-axis
			float q = Mathf.Lerp(y3, y0, negativeIntraTilePoint.x / negativeIntraTilePoint.y);
			return Mathf.Lerp(y2, q, 1 - intraTileZ * inverseScale); //lerps along the line through the point. scaled to y
		}
	}

	public void DrawMapInEditor()
	{
		m_terrainEquation.Initialise(m_seed);
		var mapData = GenerateMapData(Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (m_drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
		}
		else if (m_drawMode == DrawMode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, MapChunkSize, MapChunkSize, null));
		}
		else if (m_drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, m_editorPreviewLod,
					m_meshHeightMultiplier, m_meshHeightCurve, m_useFlatShading),
					TextureGenerator.TextureFromColourMap(mapData.colourMap, MapChunkSize, MapChunkSize, null));
		}
	}


	public void RequestMapData(Vector2 centre, Action<MapData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MapDataThread(centre, callback);
		};

		new Thread(threadStart).Start();
	}


	private void MapDataThread(Vector2 centre, Action<MapData> callback)
	{
		var mapData = GenerateMapData(centre);
		lock (m_mapDataThreadInfoQueue)
		{
			m_mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}


	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate
		{
			MeshDataThread(mapData, lod, callback);
		};

		new Thread(threadStart).Start();
	}


	private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
	{
		var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, lod,
						m_meshHeightMultiplier, m_meshHeightCurve, m_useFlatShading);

		lock (m_meshDataThreadInfoQueue)
		{
			m_meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}


	public void Initialise()
	{
		if (!m_initialised)
		{
			m_initialised = true;

			m_seed = m_useGlobalSeed ? SeedManager.TerrainSeed : m_seed;
			//print("Terrain seed: " + m_seed);

			m_terrainEquation.Initialise(m_seed);
		}
	}


	void Awake()
	{
		Initialise();
	}


	void Update()
	{
		if (m_mapDataThreadInfoQueue.Count > 0)
		{
			//print("Time: " + Time.time + ", Map data thread queue: " + m_mapDataThreadInfoQueue.Count);

			for (int i = 0; i < m_mapDataThreadInfoQueue.Count; i++)
			{
				var threadInfo = m_mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}

			//if (m_mapDataCoroutine != null)
			//    StopCoroutine(m_mapDataCoroutine);

			//m_mapDataCoroutine = StartCoroutine(ProcessMapDataQueue());
		}

		if (m_meshDataThreadInfoQueue.Count > 0)
		{
			//print("Time: " + Time.time + ", Mesh data thread queue: " + m_meshDataThreadInfoQueue.Count);

			for (int i = 0; i < m_meshDataThreadInfoQueue.Count; i++)
			{
				var threadInfo = m_meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}

			//if (m_meshDataCoroutine != null)
			//    StopCoroutine(m_meshDataCoroutine);
			//m_meshDataCoroutine = StartCoroutine(ProcessMeshDataQueue());
		}

		//if (Time.deltaTime > 0.033)
		//    print("Time: " + Time.time + ", delta time: " + Time.deltaTime);
	}


	private MapData GenerateMapData(Vector2 centre)
	{
		float[,] heightMap = GenerateHeightMap(MapChunkSize, MapChunkSize, m_terrainEquation, centre);

		Color[] colourMap = new Color[MapChunkSize * MapChunkSize];
		for (int y = 0; y < MapChunkSize; y++)
		{
			for (int x = 0; x < MapChunkSize; x++)
			{
				float currentHeight = heightMap[x, y];
				for (int i = 0; i < regions.Length; i++)
				{
					if (currentHeight >= regions[i].height)
					{
						colourMap[y * MapChunkSize + x] = regions[i].colour;
					}
					else
						break;
				}
			}
		}

		return new MapData(heightMap, colourMap);
	}


	private static float[,] GenerateHeightMap(int mapWidth, int mapHeight, ITerrainEquation terrainEquation, Vector2 centre)
	{
		float[,] map = new float[mapWidth, mapHeight];

		//float maxNoiseHeight = float.MinValue;
		//float minNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				float sampleX = centre.x - halfWidth + x;
				float sampleY = centre.y - halfHeight + y;

				float value = terrainEquation.GetHeight(sampleX, sampleY);

				//if (noiseHeight > maxNoiseHeight)
				//{
				//    maxNoiseHeight = noiseHeight;
				//}
				//else if (noiseHeight < minNoiseHeight)
				//{
				//    minNoiseHeight = noiseHeight;
				//}
				map[x, y] = value;
			}
		}

		//for (int y = 0; y < mapHeight; y++)
		//{
		//    for (int x = 0; x < mapWidth; x++)
		//    {
		//        map[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, map[x, y]);
		//    }
		//}

		return map;
	}


	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}


[Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}


public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;


	public MapData(float[,] heightMap, Color[] colourMap)
	{
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}