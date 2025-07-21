using UnityEngine;
using UnityEngine.SceneManagement;

public class Skull : MonoBehaviour, IInteractable
{
    public GameObject[] cuchillas;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact(GameObject interactor)
    {
        foreach (GameObject cuchilla in cuchillas)
        {
            Cuchillas cuchillasScript = cuchilla.GetComponent<Cuchillas>();
            if (cuchillasScript != null)
            {
                cuchillasScript.StopCuchillas(); // Detiene las cuchillas
            }
        }
    }
}
