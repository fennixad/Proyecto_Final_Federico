using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    public string requiredKeyID; // ID de la llave necesaria para abrir la puerta
    public PlayerInventory playerInventory; // Referencia al inventario del jugador (opcional, si se necesita verificar la llave)

    public void Interact(GameObject interactor)
    {
        if (!isOpen)
        {
            // Comprobar si requiere llave y si el jugador la tiene
            if (!string.IsNullOrEmpty(requiredKeyID))
            {
                if (playerInventory != null && playerInventory.HasKey(requiredKeyID))
                {
                    Debug.Log("deberia abrir");
                    OpenDoor();
                }
                else
                {
                    Debug.Log("Necesitas la llave: " + requiredKeyID + " para abrir esta puerta.");
                }
            }
            else // No requiere llave
            {
                OpenDoor();
            }
        }
        else
        {
            //CloseDoor(); // O simplemente indicar que ya está abierta
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        Debug.Log($"La puerta {gameObject.name} se ha abierto.");
        GetComponent<Animator>().SetTrigger("Open");
    }
    /*
    private void CloseDoor()
    {
        isOpen = false;
        Debug.Log($"La puerta {gameObject.name} se ha cerrado.");
        // Lógica visual/animación para cerrar la puerta
    }
    */
}
