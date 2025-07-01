using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
        SoundManager.Instance.PlaySounds(0);
    }
    public void ExitButton()
    {
        SoundManager.Instance.PlaySounds(0);
        Application.Quit();
    }
}
