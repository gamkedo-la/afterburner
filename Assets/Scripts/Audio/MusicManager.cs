using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	private static MusicManager instance = null;

	[SerializeField] AudioSource musicIntro;
	[SerializeField] AudioSource musicLoop;
	[SerializeField] AudioSource musicTrack1917;
	[SerializeField] AudioSource musicTrack1992;
	[SerializeField] AudioSource musicTrack2067;
	[SerializeField] AudioSource musicTrack2142;


	void Awake() 
	{
        if (instance == null)
		{
            //print("Music instance null: starting new one: " + this.music.name);
            instance = this;
			DontDestroyOnLoad(gameObject);
			musicLoop = musicIntro;

			/*if (instance.musicIntro != null)
			{
				musicLoop = musicIntro;
				instance.musicIntro.Play();

				if (instance.musicLoop != null)
					instance.musicLoop.PlayDelayed(instance.musicIntro.clip.length);
			}
			else if (instance.musicLoop != null)*/
			instance.musicLoop.Play();
		}
		else if (instance != this || (musicIntro == null && musicLoop == null))
		{
			//print("Destroy new music source.");
			Destroy(gameObject);
		} 
	}

	public void swapTrack(int track)
	{
		Debug.Log("Swapping to track: " + track);
		instance.musicLoop.Stop();
		switch(track)
		{
			case 0:
				musicLoop = musicIntro;
				break;
			case 1:
				musicLoop = musicTrack1917;
				break;
			case 2:
				musicLoop = musicTrack1992;
				break;
			case 3:
				musicLoop = musicTrack2067;
				break;
			case 4:
				musicLoop = musicTrack2142;
				break;
    }
		instance.musicLoop.Play();
	}
}
