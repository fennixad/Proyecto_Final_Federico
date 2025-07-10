using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentGameState { get; private set; } = GameState.Initial_Menu;

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("GameManager listo!");

            Instance = this;
            DontDestroyOnLoad(gameObject);
     
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("GameManager ya creado, se destruye");
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.Instance.PlaySounds(0);
            Instance.PauseGame();
        }
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
    }
    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        Debug.Log("Cambiando estado del juego a: " + newState);


        switch (newState)
        {
            case GameState.Initial_Menu:
                Debug.Log("Estado del juego: Menu Inicial");
                break;
            case GameState.Charging:
                Debug.Log("Estado del juego: Cargando...");
                break;
            case GameState.Playing:
                Playing();
                break;
            case GameState.Pause:
                
                Debug.Log("Estado del juego: Pausa");
                //MusicManager.Instance.PlayMusic(1, .125f, true);
                PauseManager.Instance.MenuHallVisibility(true);
                Time.timeScale = 0f;
                break;
            case GameState.GameOver:
                //Sonido muerte
                SceneManager.LoadScene(2);
                break;
            case GameState.Nivel_Completed:
                Debug.Log("Estado del juego: Nivel Completado");
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
        if (PauseManager.Instance.menuHall.activeSelf)
        {
            PauseManager.Instance.MenuHallVisibility(false);
        }
        Debug.Log("Estado del juego: Jugando");
        Time.timeScale = 1f;
        //MusicaManager.Instance.PlayMusic(0, .125f, true);
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
