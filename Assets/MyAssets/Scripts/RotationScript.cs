using UnityEngine;

public class RotationScript : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 40 * Time.deltaTime); // Rota el objeto alrededor del eje Y a 20 grados por segundo
    }
}
