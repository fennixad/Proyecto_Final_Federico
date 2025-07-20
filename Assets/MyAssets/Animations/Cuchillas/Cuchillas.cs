using UnityEngine;
using UnityEngine.SceneManagement;

public class Cuchillas : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StopCuchillas()
    {
        animator.speed = 0f; // Detiene las cuchillas
        gameObject.GetComponent<Collider>().enabled = false; // Desactiva el collider
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Cuchillas colisionando con: " + other.name);
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(2);
        }
    }
}
