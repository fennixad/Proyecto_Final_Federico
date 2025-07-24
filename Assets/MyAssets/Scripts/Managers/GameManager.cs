using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentGameState { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;           
            transform.GetChild(0).gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("GameManager ya creado, se destruye");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        SetGameState(GameState.Initial_Menu);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.Instance.PlaySounds(0);
            Instance.PauseGame();
        }
    }
    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        Debug.Log("Cambiando estado del juego a: " + newState);


        switch (newState)
        {
            case GameState.Initial_Menu:
                Time.timeScale = 1;
                MusicManager.Instance.PlayMusic(0, 0.125f, true);
                break;
            case GameState.Charging:
                break;
            case GameState.Playing:
                MusicManager.Instance.PlayMusic(1, 0.125f, true);
                Playing();
                break;
            case GameState.Pause:
                MusicManager.Instance.ChangeVolumen(0.05f);
                PauseManager.Instance.MenuHallVisibility(true);
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                MusicManager.Instance.PlayMusic(2, 0.125f, true);
                SceneManager.LoadScene(2);
                break;
            case GameState.Nivel_Completed:
                MusicManager.Instance.PlayMusic(3, 0.125f, true);
                SceneManager.LoadScene(3);
                break;
            default:
                Debug.LogError("Estado del juego no reconocido: " + newState);
                break;
        }
    }
    public void PauseGame()
    {
        if (CurrentGameState == GameState.Initial_Menu)
        {
            SetGameState(GameState.Pause);
        }
        else if (CurrentGameState == GameState.Playing)
        {
            PauseManager.Instance.MenuHallVisibility(true);
            SetGameState(GameState.Pause);
        }
        else if (CurrentGameState == GameState.Pause)
        {
            PauseManager.Instance.MenuHallVisibility(false);
            SetGameState(GameState.Playing);
        }
    }

    void Playing() 
    { 
        if (PauseManager.Instance != null)
        {
            if (PauseManager.Instance.menuHall.activeSelf)
            {
                PauseManager.Instance.MenuHallVisibility(false);
            }
        }
        Time.timeScale = 1f;
    }
    public void PlayableSceneLoad()
    {
        SceneManager.LoadScene(1);
        SetGameState(CurrentGameState);
    }

    public void MainMenuSceneLoad()
    {
        SceneManager.LoadScene(0);
        SetGameState(GameState.Initial_Menu);
        Time.timeScale = 1f;
    }
    public enum GameState
    {
        None = 0,
        Initial_Menu = 1,
        Charging = 2,
        Playing = 3,
        Pause = 4,
        GameOver = 5,
        Nivel_Completed = 6
    }
}
