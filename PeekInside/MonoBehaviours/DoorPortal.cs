using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Objects;
using GameNetcodeStuff;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    #region Unity Editor
    [SerializeField]
    private MeshRenderer _screenMeshRenderer;

    [SerializeField]
    private GameObject _cameraContainer;

    [SerializeField]
    private Camera _camera;

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

    private MainEntranceInfo _mainEntrance;

    private RenderTexture _outputRenderTexture;
    private Material _outputMaterial;

    private Material _inputMaterial;

    public void SetMainEntranceInfo(MainEntranceInfo mainEntrance)
    {
        _mainEntrance = mainEntrance;
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
        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
        {
            _outputRenderTexture = _outsideOutputRenderTexture;
            _outputMaterial = _outsideOutputMaterial;
        }
        else
        {
            _outputRenderTexture = _insideOutputRenderTexture;
            _outputMaterial = _insideOutputMaterial;
        }

        _camera.targetTexture = _outputRenderTexture;
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

        if (_mainEntrance.HasViewBlocker)
        {
            _mainEntrance.ViewBlockerObject.SetActive(!value);
        }
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

        // 1. Find the player's transform relative to the LINKED portal
        Transform linkedTransform = LinkedPortal.transform;

        // 2. Calculate the relative offset of the player camera to the linked portal
        Matrix4x4 relativeMatrix = transform.localToWorldMatrix
            * linkedTransform.worldToLocalMatrix
            * playerCamera.transform.localToWorldMatrix;

        // 3. Apply that offset to this portal's camera
        _camera.transform.SetPositionAndRotation(
            relativeMatrix.GetColumn(3),
            relativeMatrix.rotation
        );

        _camera.fieldOfView = playerCamera.fieldOfView;

        // 4. Apply oblique near-plane clipping so geometry behind the
        //    portal entrance doesn't bleed into the render
        SetObliqueNearPlane(playerCamera);
    }

    private void SetObliqueNearPlane(Camera playerCamera)
    {
        // Get the portal plane in camera-local space
        Transform clipPlane = LinkedPortal.transform;

        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, clipPlane.position - _camera.transform.position));

        Vector3 camSpacePos = _camera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = _camera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;

        float camSpaceDist = -Vector3.Dot(camSpacePos, camSpaceNormal);

        var clipPlaneCamSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDist);

        _camera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCamSpace);
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
