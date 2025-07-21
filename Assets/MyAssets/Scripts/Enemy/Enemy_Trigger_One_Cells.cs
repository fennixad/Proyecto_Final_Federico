using UnityEngine;

public class Enemy_Trigger_One_Cells : MonoBehaviour
{
    public GameObject enemyOne;
    public GameObject enemyTwo;
    public GameObject enemyThree;
    public GameObject enemyFour;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyOne.SetActive(true);
            enemyTwo.SetActive(true);
            if (enemyThree != null) enemyThree.SetActive(true);
            if (enemyFour != null) enemyFour.SetActive(true);
        }
    }
}
