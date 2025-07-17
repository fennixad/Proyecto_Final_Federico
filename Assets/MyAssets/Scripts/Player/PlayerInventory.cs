using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<string> collectedKeys = new List<string>();

    public void AddKey(string keyId)
    {
        if (!collectedKeys.Contains(keyId))
        {
            collectedKeys.Add(keyId);
            Debug.Log($"Inventario: Llave '{keyId}' añadida.");
        }
    }

    public bool HasKey(string keyId)
    {
        return collectedKeys.Contains(keyId);
    }
}
