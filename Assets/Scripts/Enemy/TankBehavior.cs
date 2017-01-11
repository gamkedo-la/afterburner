using UnityEngine;
using System.Collections;

public class TankBehavior : MonoBehaviour
{
	private static MapGenerator m_mapGenerator;
	private float scale;

	private Vector2 startPos, targetPos;
	public float roamingDistance = 250f, groundSpeed = 10f, rotationSpeed = 30f;

	private EnemyHealth health;

	void Start()
	{
		health = GetComponent<EnemyHealth>();

		var mapGeneratorObject = GameObject.FindGameObjectWithTag(Tags.MapGenerator);

		if(mapGeneratorObject != null)
			m_mapGenerator = mapGeneratorObject.GetComponent<MapGenerator>();

		if(m_mapGenerator == null)
		{
			print(string.Format("No object with the tag '{0}' was found, so object placement couldn't be performed",
					Tags.MapGenerator));
			return;
		}

		m_mapGenerator.Initialise();
		scale = m_mapGenerator.uniformScale * 2;

		startPos = new Vector2(transform.position.x, transform.position.z);
		pickTarget();
	}

	void Update()
	{
		float x = transform.position.x;
		float z = transform.position.z;
		Vector2 twoDimentionalPos = new Vector2(x, z);
		Vector2 heading = targetPos - twoDimentionalPos;

		//If we are not at the target position, move towards it
		if(!health.IsDead && heading.magnitude > groundSpeed * 1.1f)
		{
			twoDimentionalPos += heading.normalized * groundSpeed;
			x = twoDimentionalPos.x;
			z = twoDimentionalPos.y;

			moveForward(x, z, heading.normalized);
		}
		else
		{
			pickTarget();
		}
		//TODO do something if we ARE at targetPos or just have another behavior sate
	}

	//TODO add delta time
	private void moveForward(float x, float z, Vector2 heading)
	{
		Vector3 terrainNormal = m_mapGenerator.GetTerrainNormal(x, z);
		Vector2 facing = new Vector2(transform.forward.x, transform.forward.z).normalized;

		//If the terrain slope is too great, don't move and pick another target
		if(Vector3.Cross(terrainNormal, Vector3.up).sqrMagnitude > 0.04f)
		{
			pickTarget();
			return;
		}

		if(Vector2.Angle(heading, facing) > rotationSpeed * Time.deltaTime)
		{
			rotateInPlace(facing, heading);
			return;
		}

		Vector3 addedPos = (new Vector3(x, m_mapGenerator.getRealHeight(x, z), z) - transform.position) * Time.deltaTime;
		transform.position += addedPos;
		transform.rotation = Quaternion.LookRotation(terrainNormal);
		transform.rotation *= Quaternion.AngleAxis(90, Vector3.right);

		facing = new Vector2(transform.forward.x, transform.forward.z).normalized;
		correctRotation(facing, heading);
	}

	//Snap facing to heading
	private void correctRotation(Vector2 facing, Vector2 heading)
	{
		Vector3 cross = Vector3.Cross(facing, heading);
		float angle = Vector2.Angle(heading, facing);

		angle = -angle * Mathf.Sign(cross.z);
		transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
	}

	//Slowly rotate facing to heading
	private void rotateInPlace(Vector2 facing, Vector2 heading)
	{
		Vector3 cross = Vector3.Cross(facing, heading);
		float angle = Vector2.Angle(heading, facing);

		angle = -Mathf.Min(angle, rotationSpeed * Time.deltaTime) * Mathf.Sign(cross.z);
		transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
	}

	private void pickTarget()
	{
		targetPos = new Vector2(Random.Range(-roamingDistance, roamingDistance), Random.Range(-roamingDistance, roamingDistance)) + startPos;
	}
}
