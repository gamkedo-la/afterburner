using UnityEngine;

public class RotateWreckage : MonoBehaviour
{
	[SerializeField]Transform transformToRotateWith;

	void OnEnable()
	{
		transform.rotation = Quaternion.Euler(-90, 0, 0) * transformToRotateWith.rotation;
	}
}
