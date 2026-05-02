using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Objects;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    #region Unity Editor
    [SerializeField]
    private MeshRenderer _screenMeshRenderer; // The screen for this portal.

    [SerializeField]
    private GameObject _cameraContainer;

    [SerializeField]
    private Camera _camera; // The camera that renders the view of this portal.

    [Space(10f)]
    [Header("Portal Outside")]
    [Space(5f)]

    [SerializeField]
    private RenderTexture _outsideOutputRenderTexture;

    [SerializeField]
    private Material _outsideOutputMaterial;

    [Space(10f)]
    [Header("Portal Inside")]
    [Space(5f)]

    [SerializeField]
    private RenderTexture _insideOutputRenderTexture;

    [SerializeField]
    private Material _insideOutputMaterial;
    #endregion

    public DoorPortal LinkedPortal { get; private set; }
    public bool HasLinkedPortal => LinkedPortal != null;

    public bool IsCameraRendering => _cameraContainer != null && _cameraContainer.activeSelf;

    private MainEntranceData _mainEntrance;

    private RenderTexture _outputRenderTexture;
    private Material _outputMaterial;

    private Material _inputMaterial;

    public void SetMainEntranceData(MainEntranceData mainEntrance)
    {
        _mainEntrance = mainEntrance;

        if (_mainEntrance == null)
            return;

        HDAdditionalCameraData additionalCameraData = _camera.GetComponent<HDAdditionalCameraData>();

        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
        {
            _outputRenderTexture = _outsideOutputRenderTexture;
            _outputMaterial = _outsideOutputMaterial;

            additionalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
        }
        else
        {
            _outputRenderTexture = _insideOutputRenderTexture;
            _outputMaterial = _insideOutputMaterial;

            additionalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        }

        _camera.targetTexture = _outputRenderTexture;
    }

    public void LinkPortal(DoorPortal other)
    {
        if (HasLinkedPortal)
            return;

        if (this == other)
            return;

        if (other.HasLinkedPortal && other.LinkedPortal != this)
            return;

        LinkedPortal = other;
        _inputMaterial = other._outputMaterial;
        _screenMeshRenderer.material = _inputMaterial;

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked portal {_mainEntrance.EntranceTeleport.GetLogInfo()} -> {other._mainEntrance.EntranceTeleport.GetLogInfo()}");
    }

    private void Awake()
    {
        SetScreenEnabled(false);
        SetRenderingEnabled(false);
    }

    private void Update()
    {
        if (!HasLinkedPortal)
            return;

        UpdateScreen();
    }

    private void UpdateScreen()
    {
        if (IsLocalPlayerCameraNearby())
        {
            LinkedPortal.SetRenderingEnabled(true);
            SetScreenEnabled(true);
        }
        else
        {
            SetScreenEnabled(false);
            LinkedPortal.SetRenderingEnabled(false);
        }
    }

    private bool IsLocalPlayerCameraNearby()
    {
        Camera playerCamera = GetLocalPlayerCamera();
        if (playerCamera == null) return false;

        Vector3 cameraPosition = playerCamera.transform.position;

        float distance = Vector3.Distance(cameraPosition, transform.position);

        return distance <= 10f;
    }

    private void SetRenderingEnabled(bool value)
    {
        _cameraContainer?.SetActive(value);
    }

    private void SetScreenEnabled(bool value)
    {
        _screenMeshRenderer.gameObject.SetActive(value);

        if (_mainEntrance == null)
            return;

        if (_mainEntrance.HasViewBlocker)
        {
            if (IsCameraRendering)
            {
                _mainEntrance.ViewBlockerObject.SetActive(false);
            }
            else
            {
                _mainEntrance.ViewBlockerObject.SetActive(!value);
            }
        }

        // TODO: Add this back in later
        //_mainEntrance.SetDoorObjectsEnabled(!IsCameraRendering);

        _mainEntrance.SetDoorObjectsEnabled(false);
    }

    private void LateUpdate()
    {
        if (!HasLinkedPortal)
            return;

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (!_camera.gameObject.activeSelf)
            return;

        if (!_camera.enabled)
            return;

        Camera playerCamera = GetLocalPlayerCamera();
        if (playerCamera == null) return;

        Transform linkedScreen = LinkedPortal._screenMeshRenderer.transform;
        Transform thisScreen = _screenMeshRenderer.transform;

        // --- 1. Position & Rotation (unchanged, already correct) ---
        Matrix4x4 linkedFlipped = linkedScreen.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 180f, 0f));

        Matrix4x4 relativeTransform = thisScreen.localToWorldMatrix * Matrix4x4.Inverse(linkedFlipped);

        _camera.transform.SetPositionAndRotation(
            relativeTransform.MultiplyPoint(playerCamera.transform.position),
            relativeTransform.rotation * playerCamera.transform.rotation
        );

        // --- 2. Oblique Near-Clip Projection ---
        SetObliqueNearClipPlane(playerCamera);
    }

    private void SetObliqueNearClipPlane(Camera playerCamera)
    {
        Transform screenTransform = _screenMeshRenderer.transform;

        // Step 1: Build an off-axis projection matrix whose frustum edges
        // are pinned to the exact bounds of this portal's screen.
        // This ensures the RenderTexture only captures pixels visible through
        // the screen rect, with correct perspective depth.
        Matrix4x4 offAxisMatrix = CalculateOffAxisProjectionMatrix(playerCamera);

        // Step 2: Apply the oblique near-clip on top of the off-axis matrix.
        // We temporarily assign it so CalculateObliqueMatrix has a correct base to work from.
        _camera.projectionMatrix = offAxisMatrix;

        Plane clipPlane = new Plane(screenTransform.forward, screenTransform.position);

        Vector4 clipPlaneVec = new Vector4(
            clipPlane.normal.x,
            clipPlane.normal.y,
            clipPlane.normal.z,
            clipPlane.distance
        );

        Vector4 clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(_camera.worldToCameraMatrix)) * clipPlaneVec;

        _camera.projectionMatrix = _camera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private Matrix4x4 CalculateOffAxisProjectionMatrix(Camera playerCamera)
    {
        Transform screenTransform = _screenMeshRenderer.transform;
        Vector3 camPos = _camera.transform.position;

        // Get the four corners of the screen quad in world space.
        Vector3 scale = screenTransform.lossyScale;
        float halfW = scale.x * 0.5f;
        float halfH = scale.y * 0.5f;

        Vector3 bl = screenTransform.position - screenTransform.right * halfW - screenTransform.up * halfH;
        Vector3 br = screenTransform.position + screenTransform.right * halfW - screenTransform.up * halfH;
        Vector3 tl = screenTransform.position - screenTransform.right * halfW + screenTransform.up * halfH;

        // Express the corner vectors in the portal camera's local space.
        Vector3 camRight = _camera.transform.right;
        Vector3 camUp = _camera.transform.up;
        Vector3 camForward = _camera.transform.forward;

        Vector3 toBL = bl - camPos;
        Vector3 toBR = br - camPos;
        Vector3 toTL = tl - camPos;

        float nearClip = playerCamera.nearClipPlane;
        float farClip = playerCamera.farClipPlane;

        // Project each corner onto the camera forward axis to get its depth.
        // Then scale each corner's lateral offset to the near clip plane (similar triangles).
        // Use each corner's own depth so the frustum edges exactly hit the screen corners,
        // regardless of the camera's angle relative to the screen.
        float left = Vector3.Dot(toBL, camRight) / Vector3.Dot(toBL, camForward) * nearClip;
        float right = Vector3.Dot(toBR, camRight) / Vector3.Dot(toBR, camForward) * nearClip;
        float bottom = Vector3.Dot(toBL, camUp) / Vector3.Dot(toBL, camForward) * nearClip;
        float top = Vector3.Dot(toTL, camUp) / Vector3.Dot(toTL, camForward) * nearClip;

        return Matrix4x4.Frustum(left, right, bottom, top, nearClip, farClip);
    }

    private static Camera GetLocalPlayerCamera()
    {
        PlayerControllerB playerScript = PlayerUtils.LocalPlayerScript;

        if (playerScript != null && !playerScript.isPlayerDead)
        {
            return playerScript.gameplayCamera;
        }

        return StartOfRound.Instance.spectateCamera;
    }
}
