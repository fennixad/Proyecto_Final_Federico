using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject confirmationMenu;
    public void Button_Play()
    { 
        SceneManager.LoadScene(1);
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }
    public void Button_Exit()
    {
        SoundManager.Instance.PlaySounds(0);
        if (confirmationMenu != null)
        {
            pauseMenu.SetActive(false);
            confirmationMenu.SetActive(true);
        }
    }
    public void Button_Resume()
    {
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    public void Button_Yes()
    {  
        SoundManager.Instance.PlaySounds(0);
        SceneManager.LoadScene(0);
    }
    public void Button_No()
    {
        SoundManager.Instance.PlaySounds(0);
        confirmationMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }
}
