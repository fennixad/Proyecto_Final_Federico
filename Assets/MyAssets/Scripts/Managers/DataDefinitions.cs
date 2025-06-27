using System;
using UnityEngine;

public class DataDefinitions : MonoBehaviour
{
    public static DataDefinitions Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public enum InteractableObjectType
    {
        Item,
        Enemy,
        Door
    }

}
