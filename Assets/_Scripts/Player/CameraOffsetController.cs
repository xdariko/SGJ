using Cinemachine;
using UnityEngine;

public class CameraOffsetController : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 offsetAmount = new Vector2(1, 1);
    public float smoothTime = 0.2f;

    private CinemachineVirtualCamera virtualCamera;
    private Vector2 currentOffset;
    private Vector2 targetOffset;
    private Vector2 refVelocity;

    private CinemachineCameraOffset cameraOffset;

    private void Awake()
    {
        FindVirtualCamera();
        InitializeCameraOffset();
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnMove += HandleMove;
        }
        else
        {
            Debug.LogError("PlayerController instance not found!", this);
        }
    }

    private void FindVirtualCamera()
    {
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera not found in scene!", this);
            enabled = false;
        }
    }

    private void InitializeCameraOffset()
    {
        if (virtualCamera == null) return;

        cameraOffset = virtualCamera.GetComponent<CinemachineCameraOffset>();
        if (cameraOffset == null)
        {
            cameraOffset = virtualCamera.gameObject.AddComponent<CinemachineCameraOffset>();
            virtualCamera.AddExtension(cameraOffset);
        }
    }

    private void HandleMove(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            targetOffset = direction * offsetAmount;
        }
    }

    private void Update()
    {
        if (virtualCamera == null || cameraOffset == null) return;

        currentOffset = Vector2.SmoothDamp(
            currentOffset,
            targetOffset,
            ref refVelocity,
            smoothTime
        );

        cameraOffset.m_Offset = new Vector3(
            currentOffset.x,
            currentOffset.y,
            0f
        );
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnMove -= HandleMove;
        }
    }
}