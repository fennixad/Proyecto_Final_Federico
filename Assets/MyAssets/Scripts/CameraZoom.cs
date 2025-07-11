using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Zoom Config")]
    public float zoomSpeed = 5f;
    public float minDistance = 5f;
    public float maxDistance = 20f;

    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;

    void Start()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
        positionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();

        if (positionComposer == null)
        {
            Debug.LogError("CinemachinePositionComposer not found. Make sure the camera uses Position Composer.");
        }
    }

    void Update()
    {
        if (positionComposer == null) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newDistance = positionComposer.CameraDistance - scroll * zoomSpeed;
            newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
            positionComposer.CameraDistance = newDistance;
        }
    }
}
