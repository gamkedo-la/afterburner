using UnityEngine;
using System.Collections;

public class MusicSwapper : MonoBehaviour
{
	public int track = 2;
	private MusicManager musicManager;

	void Start()
	{
		musicManager = FindObjectOfType<MusicManager>();
		musicManager.swapTrack(track);
	}
}
