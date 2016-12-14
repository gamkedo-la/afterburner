using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


public class EndlessTerrain : MonoBehaviour
{
    private const float ViewerMoveThresholdForChunkUpdate = 25f;
    private const float SquViewerMoveThresholdForChunkUpdate = ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;

    [SerializeField] bool m_addCollider = true;
    [SerializeField] int m_colliderTilesToUpdatePerFrame = 1;
    [SerializeField] int m_tilesToMovePerFrame = 1;
    private int m_tilesToMoveMultiplier = 1;
    public LodInfo[] m_detailLevels;
    private LodInfo[] m_startDetailLevels;
    public static float MaxViewDst = 0f;
    public static Vector3 ViewerPosition;
    private static Vector3 ViewerPositionOld;

    private static bool m_updateTerrainDetail;
    private static MapGenerator m_mapGenerator;

    private Transform m_viewer;
    [SerializeField] Material m_mapMaterial;

    public static int ChunkSize;
    private int m_chunksVisibleInViewDst;
    private int m_chunksPerSide;

    private List<TerrainChunk> m_terrainChunks = new List<TerrainChunk>();
    private List<TerrainChunk> m_terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    private List<TerrainChunk> m_terrainChunksThatNeedMoving = new List<TerrainChunk>();
    private List<TerrainChunk> m_terrainChunksWithCollider = new List<TerrainChunk>();

    private Coroutine m_updateVisibleChunks;
    private bool m_updatingVisibleChunks;
    private TerrainDetail m_terrainDetail;


    void Start()
    {
        m_viewer = Camera.main.transform;
        m_mapGenerator = FindObjectOfType<MapGenerator>();
        //m_tilesToMoveMultiplier = m_mapGenerator.UseFlatShading ? 4 : 1;

        MaxViewDst = m_detailLevels[m_detailLevels.Length - 1].m_visibleDistThreshold;
        ChunkSize = MapGenerator.MapChunkSize - 1;
        m_chunksVisibleInViewDst = Mathf.RoundToInt(MaxViewDst / (ChunkSize * m_mapGenerator.uniformScale));
        m_chunksPerSide = m_chunksVisibleInViewDst * 2 + 1;

        m_startDetailLevels = new List<LodInfo>(m_detailLevels).ToArray();

        BuildTerrainChunks();

        m_terrainDetail = QualityController.TerrainDetail;
        UpdateDetailLevels((int) QualityController.TerrainDetail);
        m_updateTerrainDetail = true;
        UpdateAllVisibleChunks();    
    }


    void Update()
    {
        ViewerPosition = m_viewer.position / m_mapGenerator.uniformScale;

        if (m_updateTerrainDetail || (ViewerPositionOld - ViewerPosition).sqrMagnitude > SquViewerMoveThresholdForChunkUpdate)
        {
            //print(string.Format("Updating chunks: {0}", m_updatingVisibleChunks));
            if (!m_updatingVisibleChunks)
            {
                ViewerPositionOld = ViewerPosition;

                //UpdateVisibleChunksNonEnumerator();

                if (m_updateVisibleChunks != null)
                    StopCoroutine(m_updateVisibleChunks);

                m_updateVisibleChunks = StartCoroutine(UpdateVisibleChunksEnumerator());
            }
        }
    }


    private void UpdateDetailLevels(int terrainDetail)
    {
        for (int i = 0; i < m_detailLevels.Length; i++)
            m_detailLevels[i].m_lod = Math.Min(5, m_startDetailLevels[i].m_lod + terrainDetail);

        if (m_updateTerrainDetail)
            UpdateAllVisibleChunks();
    }


    private void BuildTerrainChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / ChunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.z / ChunkSize);

        for (int yOffset = -m_chunksVisibleInViewDst; yOffset <= m_chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -m_chunksVisibleInViewDst; xOffset <= m_chunksVisibleInViewDst; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                m_terrainChunks.Add(new TerrainChunk(viewedChunkCoord, ChunkSize, m_detailLevels, transform, m_mapMaterial, m_addCollider));
            }
        }
    }


    private void UpdateAllVisibleChunks()
    {
        //for (int i = 0; i < m_terrainChunksVisibleLastUpdate.Count; i++)
        //    m_terrainChunksVisibleLastUpdate[i].SetVisible(false);

        m_terrainChunksVisibleLastUpdate.Clear();

        for (int i = 0; i < m_terrainChunks.Count; i++)
        {
            var terrainChunk = m_terrainChunks[i];
            if (m_updateTerrainDetail)
                terrainChunk.UpdateMeshes(m_detailLevels);

            terrainChunk.m_addColliderImmediately = true;
            terrainChunk.m_terrainDetail = m_terrainDetail;
            terrainChunk.UpdateTerrainChunk();

            if (terrainChunk.IsVisible())
                m_terrainChunksVisibleLastUpdate.Add(terrainChunk);

            // This doesn't work at the start of the game for some reason
            // so have to use the hacky updateColliderImmediately flag instead
            //if (terrainChunk.m_colliderToBeAdded)
            //    terrainChunk.SetCollider();
        }
    }


    private IEnumerator UpdateVisibleChunksEnumerator()
    {
        m_updatingVisibleChunks = true;

        //for (int i = 0; i < m_terrainChunksVisibleLastUpdate.Count; i++)
        //    m_terrainChunksVisibleLastUpdate[i].SetVisible(false);

        m_terrainChunksVisibleLastUpdate.Clear();
        m_terrainChunksWithCollider.Clear();
        m_terrainChunksThatNeedMoving.Clear();

        for (int i = 0; i < m_terrainChunks.Count; i++)
        {
            var terrainChunk = m_terrainChunks[i];
            terrainChunk.m_terrainDetail = m_terrainDetail;

            if (terrainChunk.IsWithinRange(ChunkSize, m_chunksVisibleInViewDst))
            {
                terrainChunk.UpdateTerrainChunk();

                if (terrainChunk.m_colliderToBeAdded)
                {
                    //print(string.Format("Adding chunk to collider list {0}", terrainChunk.m_position));
                    m_terrainChunksWithCollider.Add(terrainChunk);
                }

                // TODO: can this be put into a yield return too?

                if (terrainChunk.IsVisible())
                    m_terrainChunksVisibleLastUpdate.Add(terrainChunk);            
            }
            else
            {
                m_terrainChunksThatNeedMoving.Add(terrainChunk);
            }          
        }

        int colliderTilesUpdated = 0;
        //print(string.Format("Adding colliders to {0} terrain chunks", m_terrainChunksWithCollider.Count));

        for (int i = 0; i < m_terrainChunksWithCollider.Count; i++)
        {
            //print(i);
            m_terrainChunksWithCollider[i].SetCollider();

            colliderTilesUpdated++;

            if (colliderTilesUpdated == (m_colliderTilesToUpdatePerFrame * m_tilesToMoveMultiplier))
            {
                //print(string.Format("{0}: {1} collider tiles updates", Time.time, colliderTilesUpdated));
                colliderTilesUpdated = 0;
                yield return null;
            }
        }

        yield return null;

        int tilesMoved = 0;

        for (int i = 0; i < m_terrainChunksThatNeedMoving.Count; i++)
        {
            var terrainChunk = m_terrainChunksThatNeedMoving[i];

            terrainChunk.MoveTerrainChunk(ChunkSize, m_chunksVisibleInViewDst, m_chunksPerSide);

            tilesMoved++;

            if (tilesMoved == (m_tilesToMovePerFrame * m_tilesToMoveMultiplier))
            {
                tilesMoved = 0;
                yield return null;
            }
        }

        m_updatingVisibleChunks = false;
        m_updateTerrainDetail = false;
    }


    //private void UpdateVisibleChunksNonEnumerator()
    //{
    //    m_updatingVisibleChunks = true;

    //    for (int i = 0; i < m_terrainChunksVisibleLastUpdate.Count; i++)
    //        m_terrainChunksVisibleLastUpdate[i].SetVisible(false);

    //    m_terrainChunksVisibleLastUpdate.Clear();
    //    var chunksThatNeedMoving = new List<TerrainChunk>();

    //    for (int i = 0; i < m_terrainChunks.Count; i++)
    //    {
    //        var terrainChunk = m_terrainChunks[i];

    //        if (terrainChunk.IsWithinRange(ChunkSize, m_chunksVisibleInViewDst))
    //        {
    //            terrainChunk.UpdateTerrainChunk();

    //            if (terrainChunk.IsVisible())
    //                m_terrainChunksVisibleLastUpdate.Add(terrainChunk);
    //        }
    //        else
    //        {
    //            chunksThatNeedMoving.Add(terrainChunk);
    //        }
    //    }

    //    for (int i = 0; i < chunksThatNeedMoving.Count; i++)
    //    {
    //        var terrainChunk = chunksThatNeedMoving[i];

    //        terrainChunk.MoveTerrainChunk(ChunkSize, m_chunksVisibleInViewDst, m_chunksPerSide);
    //    }

    //    m_updatingVisibleChunks = false;
    //    m_updateTerrainDetail = false;
    //}


    private void TerrainDetailChanged()
    {
        m_updateTerrainDetail = true;
        m_terrainDetail = QualityController.TerrainDetail;
        UpdateDetailLevels((int) QualityController.TerrainDetail);
    }


    void OnEnable()
    {
        //EventManager.StartListening(StandardEventName.UpdateTerrainDetail, TerrainDetailChanged);
    }


    void OnDisable()
    {
        //EventManager.StopListening(StandardEventName.UpdateTerrainDetail, TerrainDetailChanged);
    }


    public class TerrainChunk
    {
        public bool m_meshesUpdated;
        public Vector2 m_position;
        public bool m_colliderToBeAdded;
        public bool m_addColliderImmediately;
        public TerrainDetail m_terrainDetail;

        private GameObject m_meshObject;
        
        private Vector3 m_positionV3;
        private Bounds m_bounds;

        private MeshRenderer m_meshRenderer;
        private MeshFilter m_meshFilter;
        private MeshCollider m_meshCollider;
        private Mesh m_collderMesh;

        private LodInfo[] m_detailLevels;
        private LodMesh[] m_lodMeshes;

        private MapData m_mapData;
        private bool m_mapDataReceived;
        private int m_previousLodIndex = -1;

        private int m_xOffset;
        private int m_yOffset;

        private bool m_addCollider;
        

        public TerrainChunk(Vector2 coord, int size, LodInfo[] detailLevels, Transform parent, Material material, bool addCollider) 
        {
            m_addCollider = addCollider;
            m_position = coord * size;
            m_positionV3 = new Vector3(m_position.x, 0, m_position.y);
            m_bounds = new Bounds(m_positionV3, Vector3.one * size);

            m_meshObject = new GameObject("Terrain chunk");
            m_meshObject.tag = Tags.Ground;
            m_meshObject.layer = 19;
            m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            m_meshCollider = m_meshObject.AddComponent<MeshCollider>();
            m_meshRenderer.material = material;
            m_meshObject.transform.position = m_positionV3 * m_mapGenerator.uniformScale;
            m_meshObject.transform.parent = parent;
            m_meshObject.transform.localScale = Vector3.one * m_mapGenerator.uniformScale;
            //SetVisible(false);

            UpdateMeshes(detailLevels);      

            m_mapGenerator.RequestMapData(m_position, OnMapDataReceived);
        }


        public void UpdateMeshes(LodInfo[] detailLevels)
        {
            m_meshesUpdated = false;

            m_detailLevels = detailLevels;

            m_lodMeshes = m_lodMeshes ?? new LodMesh[m_detailLevels.Length];

            for (int i = 0; i < detailLevels.Length; i++)
            {
                var lodMesh = m_lodMeshes[i];

                if (lodMesh == null)
                    lodMesh = new LodMesh(detailLevels[i].m_lod, UpdateTerrainChunk, m_addCollider);
                else
                    lodMesh.ResetLodMesh(detailLevels[i].m_lod, UpdateTerrainChunk);

                m_lodMeshes[i] = lodMesh;
            }      
        }


        private void OnMapDataReceived(MapData mapData)
        {
            m_mapData = mapData;
            m_mapDataReceived = true;
            var texture = m_meshRenderer.material.mainTexture as Texture2D;

            texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, 
                MapGenerator.MapChunkSize, MapGenerator.MapChunkSize, texture);

            m_meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }


        public bool IsWithinRange(int chunkSize, int chunksVisibleInViewDst)
        {
            m_xOffset = Mathf.RoundToInt((m_position.x - ViewerPosition.x) / chunkSize);
            m_yOffset = Mathf.RoundToInt((m_position.y - ViewerPosition.z) / chunkSize);

            return Math.Abs(m_xOffset) <= chunksVisibleInViewDst && Math.Abs(m_yOffset) <= chunksVisibleInViewDst;
        }


        public void MoveTerrainChunk(int chunkSize, int chunksVisibleInViewDst, int chunksPerSide)
        {
            Vector2 shift = Vector2.zero;

            if (m_xOffset < -chunksVisibleInViewDst)
                shift += new Vector2(chunksPerSide, 0);
            else if (m_xOffset > chunksVisibleInViewDst)
                shift -= new Vector2(chunksPerSide, 0);

            if (m_yOffset < -chunksVisibleInViewDst)
                shift += new Vector2(0, chunksPerSide);
            else if (m_yOffset > chunksVisibleInViewDst)
                shift -= new Vector2(0, chunksPerSide);

            m_position = m_position + shift * chunkSize;
            m_positionV3 = new Vector3(m_position.x, 0, m_position.y);
            m_bounds = new Bounds(m_positionV3, Vector3.one * chunkSize);
            m_meshObject.transform.position = m_positionV3 * m_mapGenerator.uniformScale;

            m_meshCollider.sharedMesh = null;
            m_collderMesh = null;
            m_colliderToBeAdded = false;

            UpdateMeshes(m_detailLevels);

            m_mapGenerator.RequestMapData(m_position, OnMapDataReceived);
        }


        public void UpdateTerrainChunk()
        {
            if (!m_mapDataReceived)
                return;

            float viewerDstFromNearestEdge = Mathf.Sqrt(m_bounds.SqrDistance(ViewerPosition)) * m_mapGenerator.uniformScale;
            bool visible = true;// viewerDstFromNearestEdge <= MaxViewDst;

            if (m_meshCollider.sharedMesh != null)
                m_colliderToBeAdded = false;

            if (visible)
            {
                int lodIndex = 0;
                
                for (int i = 0; i < m_detailLevels.Length - 1; i++)
                {
                    if (viewerDstFromNearestEdge > m_detailLevels[i].m_visibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (!m_meshesUpdated || lodIndex != m_previousLodIndex)
                {
                    int nextLodIndex = Math.Min(m_detailLevels.Length - 1, lodIndex + 1);
                    var lodMesh = m_lodMeshes[lodIndex];
                    var nextLodMesh = m_lodMeshes[nextLodIndex];
                    
                    if (lodMesh.m_hasMesh && nextLodMesh.m_hasMesh)
                    {
                        m_meshFilter.mesh = lodMesh.m_mesh;
       
                        if (m_addCollider && lodIndex == 0 && m_meshCollider.sharedMesh == null) 
                        {
                            m_collderMesh = m_mapGenerator.UseFlatShading
                                || m_terrainDetail == TerrainDetail.SuperLow
                                ? lodMesh.m_mesh
                                : nextLodMesh.m_mesh;

                            m_colliderToBeAdded = true;

                            if (m_addColliderImmediately)   // Hacky fix for adding colliders when the game starts
                            {
                                SetCollider();
                                m_addColliderImmediately = false;
                            }
                        }

                        m_previousLodIndex = lodIndex;
                        m_meshesUpdated = true;
                    }
                    else
                    {
                        if (!lodMesh.m_hasRequestedMesh)
                            lodMesh.RequestMesh(m_mapData);

                        if (!nextLodMesh.m_hasRequestedMesh)
                            nextLodMesh.RequestMesh(m_mapData);
                    }
                }
            }

            //SetVisible(visible);
        }


        public void SetCollider()
        {
            m_meshCollider.sharedMesh = m_collderMesh;
        }


        public void SetVisible(bool visible)
        {
            m_meshObject.SetActive(visible);
        }


        public bool IsVisible()
        {
            return m_meshObject.activeSelf;
        }
    }


    class LodMesh
    {
        public Mesh m_mesh;
        public bool m_hasRequestedMesh;
        public bool m_hasMesh;
        private int m_lod;
        private Action m_updateCallback;
        private bool m_addCollider;


        public LodMesh(int lod, Action updateCallback, bool addCollider)
        {
            m_lod = lod;
            m_updateCallback = updateCallback;
            m_addCollider = addCollider;
        }


        public void ResetLodMesh(int lod, Action updateCallback)
        {
            m_hasMesh = false;
            m_hasRequestedMesh = false;
            m_lod = lod;
            m_updateCallback = updateCallback;
        }


        private void OnMeshDataReceived(MeshData meshData)
        {       
            if (m_addCollider)
                m_mesh = meshData.CreateMesh(null);
            else
                m_mesh = meshData.CreateMesh(m_mesh);

            m_hasMesh = true;

            m_updateCallback();
        }


        public void RequestMesh(MapData mapData)
        {
            m_hasRequestedMesh = true;
            m_mapGenerator.RequestMeshData(mapData, m_lod, OnMeshDataReceived);
        }
    }


    [Serializable]
    public struct LodInfo
    {
        [Range(0, 5)]
        public int m_lod;
        public float m_visibleDistThreshold;
    }
}
