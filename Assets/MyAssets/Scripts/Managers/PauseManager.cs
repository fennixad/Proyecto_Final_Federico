using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    public GameObject menuHall; // Referencia al menú de pause
    public GameObject confirmationMenu; // Referencia al menú de confirmación de salida

    private void Awake()
    {
        Instance = this;

    }

    public void MenuHallVisibility(bool _isVisible)
    {
        if (menuHall != null) menuHall.SetActive(_isVisible);
        if (confirmationMenu != null) confirmationMenu.SetActive(false);
    }
}


