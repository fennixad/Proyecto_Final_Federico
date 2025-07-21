using UnityEngine;

public class Enemy_Despawn_One_Cells : MonoBehaviour
{
    public GameObject enemyOne;
    public GameObject enemyTwo;
    public GameObject enemyThree;
    public GameObject enemyFour;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enemyOne != null) enemyOne.SetActive(false);
            if (enemyTwo != null) enemyTwo.SetActive(false);
            if (enemyThree != null) enemyThree.SetActive(false);
            if (enemyFour != null) enemyFour.SetActive(false);
        }
    }
}
