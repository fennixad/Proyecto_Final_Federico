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
                    if (interactor.transform.CompareTag("DoubleDoor"))
                    {
                        OpenDoubleDOor(interactor);
                    }
                    OpenDoor();
                }
                else
                {
                    Debug.Log("Necesitas la llave: " + requiredKeyID + " para abrir esta puerta.");
                }
            }
            else // No requiere llave
            {
                if (interactor.transform.CompareTag("DoubleDoor"))
                {
                    OpenDoubleDOor(interactor);
                }
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

    private void OpenDoubleDOor(GameObject interactor)
    {
        isOpen = true;
        Debug.Log($"La puerta doble {gameObject.name} se ha abierto.");
        interactor.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
        interactor.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Open");
        interactor.GetComponent<Collider>().enabled = false; // Desactiva el collider de la puerta doble para evitar más interacciones
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
