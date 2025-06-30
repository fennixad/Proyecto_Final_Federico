using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    AudioSource audioSource;

    public AudioClip[] sounds;
    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaySounds(int _index)
    {
        Debug.Log("Sonido de clic");
        audioSource.PlayOneShot(sounds[_index]);
    }
}
