using UnityEngine;
using System.Collections;
using System.Diagnostics;


public class TerrainTileManager : MonoBehaviour
{
    [SerializeField] GameObject m_terrainTileBuilder;
    [SerializeField] TerrainHeightEquationBase m_terrainHeightEquation;
    
    [SerializeField] int m_tilesPerSide = 20;
    [SerializeField] float m_tileSize = 100f;
    [Range(1, 100)]
    [SerializeField] int m_nodesPerTile = 10;

    private TerrainTileBuilder[,] m_tileArray;
    private Vector3 m_centre;
    private float m_size;
    private Transform m_cameraPos;

    private int m_minRow;
    private int m_maxRow;
    private int m_minCol;
    private int m_maxCol;

    private bool m_movingRow;
    private bool m_movingColumn;
    private float m_cameraCheckDist;


    void Awake()
    {
        m_cameraPos = Camera.main.transform;
        m_centre = m_cameraPos.position;
        m_centre.y = transform.position.y;
        m_size = m_tilesPerSide * m_tileSize;
        m_cameraCheckDist = m_tileSize * 0.75f;

        m_tileArray = new TerrainTileBuilder[m_tilesPerSide, m_tilesPerSide];

        for (int i = 0; i < m_tilesPerSide; i++)
        {
            for (int j = 0; j < m_tilesPerSide; j++)
            {
                var tileBuilderObject = Instantiate(m_terrainTileBuilder);
                tileBuilderObject.transform.parent = transform;
                var tileBuilder = tileBuilderObject.GetComponent<TerrainTileBuilder>();
                tileBuilder.SetNodesAndSize(m_nodesPerTile, m_tileSize);
                m_tileArray[i, j] = tileBuilder;
            }
        }

        m_minRow = 0;
        m_minCol = 0;
        m_maxRow = m_tilesPerSide - 1;
        m_maxCol = m_tilesPerSide - 1;
    }


	void Start ()
    {
        m_terrainHeightEquation.Randomise();
        BuildMap();
        StartCoroutine(CheckCameraPosition());
	}


	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.B))
        {
            m_terrainHeightEquation.Randomise();
            BuildMap();
        }
	}


    private void BuildMap()
    {
        var timer = new Stopwatch();
        timer.Start();

        float offset = m_size * 0.5f - m_tileSize * 0.5f;
        var origin = new Vector3(m_centre.x - offset, m_centre.y, m_centre.z - offset);

        for (int i = 0; i < m_tilesPerSide; i++)
        {
            for (int j = 0; j < m_tilesPerSide; j++)
            {
                float x = i * m_tileSize + origin.x;
                float z = j * m_tileSize + origin.z;
                var pos = new Vector3(x, origin.y, z);
                var tileBuilder = m_tileArray[i, j];
                tileBuilder.BuildTerrainTile(pos, m_terrainHeightEquation);
            }
        }

        timer.Stop();
        print(string.Format("Map built in {0:0.##}ms", timer.Elapsed.TotalMilliseconds));
    }


    IEnumerator CheckCameraPosition()
    {
        while(true)
        {
            float xDist = m_cameraPos.position.x - m_centre.x;
            float zDist = m_cameraPos.position.z - m_centre.z;

            if (!m_movingRow && Mathf.Abs(xDist) > m_cameraCheckDist)
                StartCoroutine(MoveRow(xDist));

            if (!m_movingColumn && Mathf.Abs(zDist) > m_cameraCheckDist)
                StartCoroutine(MoveColumn(zDist));

            yield return null;
        }
    }


    IEnumerator MoveRow(float xDist)
    {
        m_movingRow = true;

        if (Mathf.Sign(xDist) > 0)
        {
            for (int i = 0; i < m_tilesPerSide; i++)
            {
                var tile = m_tileArray[m_minRow, i];
                var centre = tile.m_centre;
                centre.x += m_size;
                tile.BuildTerrainTile(centre, m_terrainHeightEquation);

                yield return null;
            }

            m_centre.x += m_tileSize;
            m_maxRow = m_minRow;
            m_minRow++;
            if (m_minRow == m_tilesPerSide)
                m_minRow = 0;
        }
        else
        {
            for (int i = 0; i < m_tilesPerSide; i++)
            {
                var tile = m_tileArray[m_maxRow, i];
                var centre = tile.m_centre;
                centre.x -= m_size;
                tile.BuildTerrainTile(centre, m_terrainHeightEquation);

                yield return null;
            }

            m_centre.x -= m_tileSize;
            m_minRow = m_maxRow;
            m_maxRow--;
            if (m_maxRow < 0)
                m_maxRow = m_tilesPerSide - 1;
        }

        m_movingRow = false;
        yield return null;
    }


    IEnumerator MoveColumn(float zDist)
    {
        m_movingColumn = true;

        if (Mathf.Sign(zDist) > 0)
        {
            for (int i = 0; i < m_tilesPerSide; i++)
            {
                var tile = m_tileArray[i, m_minCol];
                var centre = tile.m_centre;
                centre.z += m_size;
                tile.BuildTerrainTile(centre, m_terrainHeightEquation);

                yield return null;
            }

            m_centre.z += m_tileSize;
            m_maxCol = m_minCol;
            m_minCol++;
            if (m_minCol == m_tilesPerSide)
                m_minCol = 0;
        }
        else
        {
            for (int i = 0; i < m_tilesPerSide; i++)
            {
                var tile = m_tileArray[i, m_maxCol];
                var centre = tile.m_centre;
                centre.z -= m_size;
                tile.BuildTerrainTile(centre, m_terrainHeightEquation);

                yield return null;
            }

            m_centre.z -= m_tileSize;
            m_minCol = m_maxCol;
            m_maxCol--;
            if (m_maxCol < 0)
                m_maxCol = m_tilesPerSide - 1;
        }

        m_movingColumn = false;
        yield return null;
    }
}
