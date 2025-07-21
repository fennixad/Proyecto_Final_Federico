using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHudManager : MonoBehaviour
{
    public GameObject cellKey;
    public GameObject hallKey;
    public GameObject throneKey;

    public void UpdateCellKey(string keyName)
    {
        switch (keyName)
        {
            case "CellKey":
                cellKey.SetActive(true);
                break;
            case "HallKey":
                hallKey.SetActive(true);
                break;
            case "ThroneKey":
                throneKey.SetActive(true);
                break;
            default:
                Debug.LogWarning("Key name not recognized: " + keyName);
                break;
        }
    }
}
