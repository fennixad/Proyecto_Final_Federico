using UnityEditorInternal;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public GameObject menuHall; // Referencia al menú de pause

    private void Awake()
    {
        Instance = this;
    }

    public void MenuHallVisibility(bool _isVisible)
    {
        if (menuHall != null) menuHall.SetActive(_isVisible);
    }
    public void Button_Continue()
    {
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
    }

    public void Button_Restart()
    {
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.PlayableSceneLoad();
    }

    public void Button_Exit()
    {
        SoundManager.Instance.PlaySounds(0);
        GameManager.Instance.MainMenuSceneLoad();
    }
}


