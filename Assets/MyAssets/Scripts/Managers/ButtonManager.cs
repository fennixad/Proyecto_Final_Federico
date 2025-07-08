using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene(1);
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }
    public void ExitButton()
    {
        SoundManager.Instance.PlaySounds(0);
        Application.Quit();
    }
    public void Button_Resume()
    {
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }
}
