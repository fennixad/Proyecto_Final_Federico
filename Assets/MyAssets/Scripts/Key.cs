using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    public string keyID = "MasterKey"; // ID de la llave, para identificarla

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Interactuando con la llave: {gameObject.name} desde {interactor.name}");

        // Ejemplo: Añadir la llave al inventario del jugador
        PlayerInventory playerInventory = interactor.GetComponent<PlayerInventory>();
        if (playerInventory != null)
        {
            playerInventory.AddKey(keyID);
            Debug.Log($"Jugador recogió la llave: {keyID}");
            CheckInteractable(); // Destruir la llave después de recogerla Pero no si es un enemigo.
        }
        else
        {
            Debug.LogWarning($"El jugador {interactor.name} no tiene un componente PlayerInventory.");
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
