using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    AudioSource audioSource;
    public AudioClip[] musics;

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic(int _index, float _volume, bool _loop)
    {
        if (audioSource.clip != null && audioSource.clip != musics[_index])
        {
            Debug.Log("Pista nueva, se reproduce desde le principio");
            audioSource.clip = musics[_index];
            audioSource.loop = _loop;
            audioSource.volume = _volume;
            audioSource.Play();
        }
        else
        {
            audioSource.volume = _volume;
            Debug.Log("Pista ya en uso, se omite la reproduccion desde el principio");
        }

        if (audioSource.clip == null)
        {
            audioSource.clip = musics[_index];
            audioSource.loop = _loop;
            audioSource.volume = _volume;

            audioSource.Play();
        }
    }
    public void ChangeVolumen(float _volumen)
    {
        audioSource.volume = _volumen;
    }
}
