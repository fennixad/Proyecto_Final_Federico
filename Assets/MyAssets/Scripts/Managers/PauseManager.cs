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
}


