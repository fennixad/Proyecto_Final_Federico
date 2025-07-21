using UnityEngine;

public class WinCon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.SetGameState(GameManager.GameState.Nivel_Completed);
        }
    }
}
