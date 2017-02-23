using UnityEngine;

public class ClutterSet : MonoBehaviour
{
	[SerializeField] PlacementOptionsClutter[] m_clutterOptions;

	public PlacementOptionsClutter[] getClutterSet()
	{
		return m_clutterOptions;
	}
}
