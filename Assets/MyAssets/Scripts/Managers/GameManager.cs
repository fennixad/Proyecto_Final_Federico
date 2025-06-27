using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("GameManager listo!");

            Instance = this;
            DontDestroyOnLoad(gameObject);

            //
            transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("GameManager ya creado, se destruye");
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
