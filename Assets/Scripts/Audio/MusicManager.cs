using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class MusicManager : MonoBehaviour 
{
	private static MusicManager instance = null;

	[SerializeField] AudioSource musicIntro;
	[SerializeField] AudioSource musicLoop;

	
	void Awake() 
	{
        if (instance == null)
		{
            //print("Music instance null: starting new one: " + this.music.name);
            instance = this;
			DontDestroyOnLoad(gameObject);

			if (instance.musicIntro != null)
			{	
				instance.musicIntro.Play();

				if (instance.musicLoop != null)
					instance.musicLoop.PlayDelayed(instance.musicIntro.clip.length);
			}
			else if (instance.musicLoop != null)
				instance.musicLoop.Play();
		}
		else if (instance != this || (musicIntro == null && musicLoop == null))
		{
			//print("Destroy new music source.");
			Destroy(gameObject);
		} 
	}
}
