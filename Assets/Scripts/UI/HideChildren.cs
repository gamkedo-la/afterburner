using UnityEngine;
using System.Collections;

public class HideChildren : MonoBehaviour
{
	void Update()
    {
        if (!GameController.AllowCheatMode)
            return;

	    if (Input.GetKeyDown(KeyCode.H))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i).gameObject;
                child.SetActive(!child.activeSelf);
            }             
        }
	}
}
