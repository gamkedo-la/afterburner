using UnityEngine;
using System.Collections;

public static class BoundsUtilities
{
	public static Bounds? OverallBounds(GameObject gameObject)
	{
		var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

		int length = renderers.Length;

		if (length == 0)
			return null;

		var newBounds = renderers[0].bounds;

		for (int i = 1; i < length; i++)
			newBounds.Encapsulate(renderers[i].bounds);

		return newBounds;
	}
}
