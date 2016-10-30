using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainTileBuilder : MonoBehaviour
{
    public Vector3 m_centre;

    private int m_nodes = 10;
    private float m_size = 100f;

    private ITerrainHeightEquation m_terrainHeightEquation;

    private float m_squareSize;
    private Vector3 m_origin;
    private int m_squares;

    private List<Vector3> m_vertices;
    private List<int> m_triangles;
    private List<Vector2> m_uvs;
    private List<Color32> m_colours;
    private Mesh m_mesh;


    public void SetNodesAndSize(int nodes, float size)
    {
        m_nodes = nodes;
        m_size = size;
        m_squares = m_nodes - 1;
        m_squareSize = m_size / m_squares;
    }


    public void BuildTerrainTile(Vector3 centre, ITerrainHeightEquation terrainHeightEquation)
    {
        m_centre = centre;
        transform.position = m_centre;
        m_origin = new Vector3(-m_size * 0.5f, m_centre.y, -m_size * 0.5f);
        m_terrainHeightEquation = terrainHeightEquation;

        m_vertices = new List<Vector3>();
        m_triangles = new List<int>();
        m_uvs = new List<Vector2>();
        m_colours = new List<Color32>();

        for (int i = 0; i < m_squares; i++)
        {
            for (int j = 0; j < m_squares; j++)
            {
                var square = BuildSquare(i, j);
                AddVertices(square);
                AddTriangles(square);
                AddUvs(square);
                AddColours(square);
            }
        }
        
        m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;

        m_mesh.SetVertices(m_vertices);
        m_mesh.SetTriangles(m_triangles, 0);
        m_mesh.SetUVs(0, m_uvs);
        m_mesh.SetColors(m_colours);
        m_mesh.RecalculateNormals();
    }


    private void AddVertices(Square square)
    {
        AddVertices(square.m_triangle1);
        AddVertices(square.m_triangle2);
    }


    private void AddVertices(Triangle triangle)
    {
        var nodes = triangle.m_nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node.vertexIndex == -1)
            {
                node.vertexIndex = m_vertices.Count;
                m_vertices.Add(node.m_position);
            }
        }
    }


    private void AddTriangles(Square square)
    {
        AddTriangles(square.m_triangle1);
        AddTriangles(square.m_triangle2);
    }


    private void AddTriangles(Triangle triangle)
    {
        var nodes = triangle.m_nodes;
        for (int i = 0; i < nodes.Length; i++)
        {
            m_triangles.Add(nodes[i].vertexIndex);
        }
    }


    private void AddUvs(Square square)
    {
        AddUvs(square.m_triangle1);
        AddUvs(square.m_triangle2);
    }


    private void AddUvs(Triangle triangle)
    {
        //var nodes = triangle.m_nodes;

        var uv = m_terrainHeightEquation.GetUv(triangle.m_vertices);
        m_uvs.AddRange(uv);
    }


    private void AddColours(Square square)
    {
        AddColours(square.m_triangle1);
        AddColours(square.m_triangle2);
    }


    private void AddColours(Triangle triangle)
    {
        //var nodes = triangle.m_nodes;

        var colours = m_terrainHeightEquation.GetVertexColours(triangle.m_vertices);
        m_colours.AddRange(colours);
    }


    private Square BuildSquare(int i, int j)
    {
        float x = m_centre.x;
        float z = m_centre.z;

        float x1 = i * m_squareSize + m_origin.x;
        float z1 = j * m_squareSize + m_origin.z;
        float y1 = m_terrainHeightEquation.GetHeight(x1 + x, z1 + z) + transform.position.y;
        //print("x1: " + x1 + ", z1: " + z1);

        float x2 = x1 + m_squareSize;
        float z2 = z1;
        float y2 = m_terrainHeightEquation.GetHeight(x2 + x, z2 + z) + transform.position.y;

        float x3 = x1 + m_squareSize;
        float z3 = z1 + m_squareSize;
        float y3 = m_terrainHeightEquation.GetHeight(x3 + x, z3 + z) + transform.position.y;

        float x4 = x1;
        float z4 = z1 + m_squareSize;
        float y4 = m_terrainHeightEquation.GetHeight(x4 + x, z4 + z) + transform.position.y;

        var bottomLeftPos = new Vector3(x1, y1, z1);
        var bottomRightPos = new Vector3(x2, y2, z2);
        var topRightPos = new Vector3(x3, y3, z3);
        var topLeftPos = new Vector3(x4, y4, z4);

        var bottomLeft = new Node(bottomLeftPos);
        var bottomRight = new Node(bottomRightPos);
        var topRight = new Node(topRightPos);
        var topLeft = new Node(topLeftPos);

        return new Square(topLeft, topRight, bottomRight, bottomLeft);
    }


    public class Square
    {
        public Node m_topLeft;
        public Node m_topRight;
        public Node m_bottomRight;
        public Node m_bottomLeft;

        public Triangle m_triangle1;
        public Triangle m_triangle2;

        public List<Vector3> m_vertices;

        public Square(Node topLeft, Node topRight, Node bottomRight, Node bottomLeft)
        {
            m_topLeft = topLeft;
            m_topRight = topRight;
            m_bottomRight = bottomRight;
            m_bottomLeft = bottomLeft;

            m_triangle1 = new Triangle(m_topLeft, m_topRight, m_bottomRight);
            m_triangle2 = new Triangle(new Node(m_topLeft), new Node(m_bottomRight), m_bottomLeft);

            m_vertices = new List<Vector3>();
            m_vertices.AddRange(m_triangle1.m_vertices);
            m_vertices.AddRange(m_triangle2.m_vertices);
        }
    }


    public class Triangle
    {
        public Node[] m_nodes;
        public List<Vector3> m_vertices;

        public Triangle(Node node1, Node node2, Node node3)
        {
            m_nodes = new Node[3];
            m_nodes[0] = node1;
            m_nodes[1] = node2;
            m_nodes[2] = node3;

            m_vertices = new List<Vector3>
            {
                m_nodes[0].m_position,
                m_nodes[1].m_position,
                m_nodes[2].m_position
            };
        }
    }


    public class Node
    {
        public Vector3 m_position;
        public int vertexIndex = -1;

        public Node(Vector3 position)
        {
            m_position = position;
        }


        public Node(Node other)
        {
            m_position = new Vector3(other.m_position.x, other.m_position.y, other.m_position.z);
        }
    }
}
