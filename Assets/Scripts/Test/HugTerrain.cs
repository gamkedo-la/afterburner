using UnityEngine;
using System.Collections;


using System.Collections.Generic;
public class HugTerrain : MonoBehaviour
{
	//Terribly implemented debug code for a sphere grid
	public float sphereOffsetX = 0.0f, sphereOffsetZ = 0.0f;
	private List<GameObject> sphereGrid = new List<GameObject>();
	public GameObject sphereStandin;

	private MapGenerator m_mapGenerator;
	private float scale;

	// Use this for initialization
	void Start()
	{
		var mapGeneratorObject = GameObject.FindGameObjectWithTag(Tags.MapGenerator);
		if (mapGeneratorObject != null)
			m_mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();

		if (m_mapGenerator == null)
		{
			print(string.Format("No object with the tag '{0}' was found, so object placement couldn't be performed",
					Tags.MapGenerator));
			return;
		}

		m_mapGenerator.Initialise();
		Debug.Log(m_mapGenerator.uniformScale + ", " + MapGenerator.MapChunkSize);
		scale = m_mapGenerator.uniformScale * 2;

		makeSphereGrid(); //Debug code to make a grid of spheres hug the terrain
	}

	// Update is called once per frame
	void Update()
	{
		#region sphereGrid
		//Reposition terrain hugging sphere grid

		for (int i = 0; i < sphereGrid.Count; i++)
		{
			float x = (i % 21) * scale - 10 + sphereOffsetX;
			float z = (i / 21) * scale - 10 + sphereOffsetZ;
			sphereGrid[i].transform.position = new Vector3(x, m_mapGenerator.getRealHeight(x, z), z);
			sphereGrid[i].transform.rotation = Quaternion.LookRotation(m_mapGenerator.GetTerrainNormal(x, z));
			sphereGrid[i].transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);

			Vector2 facing = new Vector2(sphereGrid[i].transform.forward.x, sphereGrid[i].transform.forward.z).normalized;
			Vector3 cross = Vector3.Cross(facing, Vector2.up);
      float angle = Vector2.Angle(Vector2.up, facing);
			if(cross.z < 0)
			{
				sphereGrid[i].transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
			}
			else
			{
				sphereGrid[i].transform.rotation *= Quaternion.AngleAxis(-angle, Vector3.up);
			}

			/*
			sphereGrid[i].transform.position = new Vector3(
				sphereGrid[i].transform.position.x + sphereOffsetX,
				m_mapGenerator.getRealHeight(sphereGrid[i].transform.position.x + sphereOffsetX, sphereGrid[i].transform.position.z + sphereOffsetZ),
				sphereGrid[i].transform.position.z + sphereOffsetZ);
			*/
		}
		sphereOffsetX -= 1;
		#endregion sphereGrid

		transform.position = new Vector3(transform.position.x, getRealHeight(transform.position.x, transform.position.z), transform.position.z);
  }

	float getRealHeight(float x, float z)
	{
    float closeX = Mathf.Floor(x / scale) * scale;
		float closeZ = Mathf.Floor(z / scale) * scale;
		float farX = (Mathf.Floor(x / scale) + 1) * scale;
		float farZ = (Mathf.Floor(z / scale) + 1) * scale;

		float intraTileX = x % scale;
		float intraTileZ = z % scale;

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

		float y0 = m_mapGenerator.GetTerrainHeight(closeX, closeZ);
		float y1 = m_mapGenerator.GetTerrainHeight(closeX, farZ);
		float y2 = m_mapGenerator.GetTerrainHeight(farX, farZ);
		float y3 = m_mapGenerator.GetTerrainHeight(farX, closeZ);

    if (aboveMidLine)
		{
			float tx = findTx(new Vector2(0, 0), new Vector2(intraTileX, intraTileZ));

			//lerps the appropiate distance along the leg of the triangle parallel to the x-axis
			float q = Mathf.Lerp(y1, y2, tx);
			return Mathf.Lerp(y0, q, intraTileZ / scale); //lerps along the line through the point. scaled to y
		}
		else
		{
			float tx = findTx(new Vector2(scale, scale), new Vector2(intraTileX, intraTileZ));

			//lerps the appropiate distance along the leg of the triangle parallel to the x-axis
			float q = Mathf.Lerp(y3, y0, tx);
			return Mathf.Lerp(y2, q, 1 - intraTileZ / scale); //lerps along the line through the point. scaled to y
		}
	}

	//Create terrain hugging sphere grid
	void makeSphereGrid()
	{
		//float gridScale = m_mapGenerator.uniformScale * 2;
		for (int x = -10; x <= 10; x++)
		{
			for(int z = -10; z <= 10; z++)
			{
				//GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				GameObject sphere = Instantiate(sphereStandin);
        sphere.transform.position = new Vector3(x * scale,
					                                      m_mapGenerator.GetTerrainHeight(x * scale, z * scale),
					                                      z * scale);
				sphereGrid.Add(sphere);
			}
		}
	}
	

	//tx is the x component of the lerp	
	float findTx(Vector2 a, Vector2 p)
	{
		Vector2 ap = p - a;
		return (ap.x / ap.y);
	}
}
