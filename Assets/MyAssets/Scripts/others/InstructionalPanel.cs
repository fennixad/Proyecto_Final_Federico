using UnityEngine;

public class InstructionalPanel : MonoBehaviour
{
    [Header("UI del mensaje")]
    public GameObject interactionUI; 

    [Header("Tag del Player")]
    public string playerTag = "Player"; 

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && interactionUI != null)
        {
            interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
}
