using Unity.Cinemachine;
using UnityEngine;

public class CameraRotationInput : MonoBehaviour
{
    public Transform cameraPivot; // el mismo que tienes como "Follow"
    public float orbitSpeed = 90f;

    private CinemachineCamera vcam;
    private CinemachineComponentBase component;

    private Vector3 currentOffset;

    void Start()
    {
        vcam = GetComponent<CinemachineCamera>();
        component = vcam.GetCinemachineComponent(CinemachineCore.Stage.Body);

        if (component is CinemachinePositionComposer composer)
        {
            currentOffset = composer.TargetOffset;
        }
        else
        {
            Debug.LogError("Camera is not using Position Composer");
        }
    }

    void Update()
    {
        if (!(component is CinemachinePositionComposer composer)) return;

        float input = 0f;

        if (Input.GetKey(KeyCode.A)) input = -1f;
        if (Input.GetKey(KeyCode.D)) input = 1f;

        if (input != 0f)
        {
            // Rotar el offset alrededor del jugador
            Quaternion rotation = Quaternion.AngleAxis(input * orbitSpeed * Time.deltaTime, Vector3.up);
            currentOffset = rotation * currentOffset;
            composer.TargetOffset = currentOffset;
        }
    }
}
