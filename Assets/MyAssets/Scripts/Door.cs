using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool isOpen = false;
    public string requiredKeyID = ""; // ID de la llave necesaria para abrir la puerta

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Interactuando con la puerta: {gameObject.name} desde {interactor.name}");

        if (!isOpen)
        {
            // Comprobar si requiere llave y si el jugador la tiene
            if (!string.IsNullOrEmpty(requiredKeyID))
            {
                PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
                if (playerInventory != null && playerInventory.HasKey(requiredKeyID))
                {
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
            CloseDoor(); // O simplemente indicar que ya está abierta
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        Debug.Log($"La puerta {gameObject.name} se ha abierto.");
        // Aquí iría tu lógica visual/animación para abrir la puerta
        // Por ejemplo, GetComponent<Animator>().SetTrigger("Open");
    }

    private void CloseDoor()
    {
        isOpen = false;
        Debug.Log($"La puerta {gameObject.name} se ha cerrado.");
        // Lógica visual/animación para cerrar la puerta
    }

    // Opcional
    void OnDrawGizmos()
    {
        Gizmos.color = isOpen ? Color.green : Color.blue;
        Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(1, 2, 0.2f));
    }
}
