using UnityEngine;

public class Enemy : MonoBehaviour, IInteractable
{
    public float damageOnInteract = 10f; // Daño que el jugador le hace al enemigo al interactuar

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Interactuando con el enemigo: {gameObject.name} desde {interactor.name}");

        // Ejemplo: Si el enemigo tiene un componente de vida
        // (Asume que tienes un script EnemyHealth)
        // EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
        // if (enemyHealth != null)
        // {
        //     enemyHealth.TakeDamage(damageOnInteract);
        //     Debug.Log($"Enemigo {gameObject.name} recibió {damageOnInteract} de daño.");
        // }
        // else
        // {
        //     Debug.LogWarning($"El enemigo {gameObject.name} no tiene un componente EnemyHealth para recibir daño.");
        // }

        // Aquí puedes añadir más lógica específica del enemigo
        // Por ejemplo, iniciar un combate, un diálogo, etc.
    }

    // Opcional: Para visualizar el componente en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
