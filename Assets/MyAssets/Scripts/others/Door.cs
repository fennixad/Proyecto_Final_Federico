using UnityEngine;
using UnityEngine.AI;

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
                    Debug.Log("Puerta: " + interactor.GetComponent<Transform>().tag);
                    if (interactor.transform.CompareTag("DoubleDoor"))
                    {
                        OpenDoubleDoor(interactor);
                    } 
                    else 
                    {
                        OpenDoor();
                    }
                        
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
                    OpenDoubleDoor(interactor);
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

    private void OpenDoubleDoor(GameObject interactor)
    {
        isOpen = true;
        Debug.Log($"La puerta doble {gameObject.name} se ha abierto.");
        interactor.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
        interactor.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = false; // Desactiva el collider de la puerta doble para evitar más interacciones
        interactor.transform.GetChild(1).GetComponent<Animator>().SetTrigger("Open");
        interactor.transform.GetChild(1).GetComponent<NavMeshObstacle>().enabled = false; // Desactiva el collider de la puerta doble para evitar más interacciones
        /*
        if (interactor.transform.GetChild(0).TryGetComponent(out Animator _animComp))
        {
            _animComp.SetTrigger("Open");
            interactor.transform.GetChild(0).GetComponent<NavMeshObstacle>().enabled = false;
        }
        if (interactor.transform.GetChild(1).TryGetComponent(out Animator _animComp2))
        {
            _animComp2.SetTrigger("Open");
            interactor.transform.GetChild(1).GetComponent<NavMeshObstacle>().enabled = false;
        }
        */
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
