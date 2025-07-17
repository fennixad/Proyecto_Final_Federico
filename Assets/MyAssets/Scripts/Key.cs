using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    public string keyID; // ID de la llave, para identificarla
    public PlayerInventory playerInventory; // Referencia al inventario del jugador
    public void Interact(GameObject interactor)
    {
        Debug.Log($"Interactuando con la llave: {gameObject.name} desde {interactor.name}");

        // Añadir la llave al inventario del jugador
        if (playerInventory != null)
        {
            playerInventory.AddKey(keyID);
            Debug.Log($"Jugador recogió la llave: {keyID}");
            CheckInteractable(); // Destruir la llave después de recogerla Pero no si es un enemigo.
        }
    }
    public void CheckInteractable()
    {
        if (!gameObject.transform.CompareTag("Enemy"))
        {
            Destroy(gameObject); 
        }
    }
}
