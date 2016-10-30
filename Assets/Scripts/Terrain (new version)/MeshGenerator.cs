using UnityEngine;
using System.Collections;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, int levelOfDetail, 
		float heightMultiplier, AnimationCurve heightCurve, bool useFlatShading)
    {
        var localHeightCurve = new AnimationCurve(heightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / -2f;

        //int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int meshSimplificationIncrement = (int) Mathf.Pow(2, levelOfDetail);
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, useFlatShading);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                float heightValue = localHeightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightValue, topLeftZ + y);	
                meshData.uvs[vertexIndex] = new Vector2((x + 0.5f) / width, (y + 0.5f) / height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex + verticesPerLine, vertexIndex + verticesPerLine + 1, vertexIndex);
                    meshData.AddTriangle(vertexIndex + 1, vertexIndex, vertexIndex + 1 + verticesPerLine);
                }

                vertexIndex++;
            }
        }

        meshData.ProcessMesh();

        return meshData;
    }
}


public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    public int m_meshSize;

    private Vector3[] bakedNormals;
    private int triangleIndex;

    private bool m_useFlatShading;


    public MeshData(int meshSize, bool useFlatShading)
    {
        m_meshSize = meshSize;
        vertices = new Vector3[meshSize * meshSize];
        uvs = new Vector2[meshSize * meshSize];
        triangles = new int[(meshSize - 1) * (meshSize - 1) * 6];
        m_useFlatShading = useFlatShading;
    }


    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }


    private Vector3[] CalculateNormals()
    {
        var vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; i++)
        {
            int normalTraingleIndex = i * 3;
            int vertexIndexA = triangles[normalTraingleIndex];
            int vertexIndexB = triangles[normalTraingleIndex + 1];
            int vertexIndexC = triangles[normalTraingleIndex + 2];

            var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
            vertexNormals[i].Normalize();

        return vertexNormals;
    }


    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        var pointA = vertices[indexA];
        var pointB = vertices[indexB];
        var pointC = vertices[indexC];

        var sideAB = pointB - pointA;
        var sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }


    public void ProcessMesh()
    {
        if (m_useFlatShading)
            FlatShading();

        BakeNormals();
    }


    private void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }


    private void FlatShading()
    {
        var flatShadedVertices = new Vector3[triangles.Length];
        var flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;
    }


    public Mesh CreateMesh(Mesh mesh)
    {
        if (mesh != null)
        	mesh.Clear();

        mesh = mesh ?? new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // RecalculateNormals seems to be more efficient than the bespoke CalculateNormals method oddly
        // But putting it on the other thread seems reasonable too
        // Need to see if removing the coroutine will be any good
        //mesh.RecalculateNormals();
        //mesh.normals = CalculateNormals();
        mesh.normals = bakedNormals;
        return mesh;
    }
}