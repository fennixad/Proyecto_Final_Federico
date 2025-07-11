using UnityEngine;

public class CameraOrbitController : MonoBehaviour
{
    [Header("Velocidad de rotación (grados/segundo)")]
    public float rotationSpeed = 120f;

    void Update()
    {
        float inputy = 0f;


        if (Input.GetKey(KeyCode.A)) inputy = -1f;
        if (Input.GetKey(KeyCode.D)) inputy = 1f;

        if (inputy != 0)
        {
            // Rota el pivot en el eje Y (vertical)
            transform.Rotate(Vector3.up, inputy * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
