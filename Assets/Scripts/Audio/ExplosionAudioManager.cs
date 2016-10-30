using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ExplosionAudioManager : MonoBehaviour
{
    [SerializeField] Vector2 m_pitchMinMax = new Vector2(0.8f, 1.2f);
    //[SerializeField] AudioClipsByTag[] m_audioClips;


    private AudioSource m_audioSource;


    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.pitch = Random.Range(m_pitchMinMax.x, m_pitchMinMax.y);
    }


    //public void SetClip(string tag)
    //{
    //    print(string.Format("Explosion audio triggered by object with tag: {0}", tag));

    //    for (int i = 0; i < m_audioClips.Length; i++)
    //    {
    //        var clip = m_audioClips[i];

    //        if (tag == clip.tag)
    //        {
    //            int index = Random.Range(0, clip.audioClips.Length);
    //            m_audioSource.clip = clip.audioClips[index];
    //            break;
    //        }
    //    }

    //    if (m_audioSource.playOnAwake)
    //        m_audioSource.Play();
    //}


    public void SetClips(AudioClip[] audioClips)
    {
        if (audioClips.Length == 0)
            return;

        int index = Random.Range(0, audioClips.Length);

        m_audioSource.clip = audioClips[index];

        if (m_audioSource.playOnAwake)
            m_audioSource.Play();
    }


    public float ClipLength
    {
        get { return m_audioSource.clip != null ? m_audioSource.clip.length : 0; }
    }
}
