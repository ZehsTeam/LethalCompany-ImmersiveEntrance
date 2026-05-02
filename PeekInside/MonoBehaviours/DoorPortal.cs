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
    private MeshRenderer _screen; // The screen for this portal.

    [SerializeField]
    private Camera _portalCamera; // The camera that renders the view of this portal.

    [SerializeField]
    private GameObject _renderingContainer;
    #endregion

    private MainEntranceData _mainEntrance;
    private DoorPortal _linkedPortal;
    private RenderTexture _viewTexture;
    private bool _isDrawing;

    private void Awake()
    {
        CreateViewTexture();

        SetDrawing(false);
        SetRendering(false);
    }

    private void Update()
    {
        if (_linkedPortal == null)
            return;

        if (IsLocalPlayerCameraNearby())
        {
            if (_isDrawing) return;

            SetDrawing(true);
            _linkedPortal.SetRendering(true);
        }
        else
        {
            if (!_isDrawing) return;

            SetDrawing(false);
            _linkedPortal.SetRendering(false);
        }
    }

    private void LateUpdate()
    {
        if (_linkedPortal == null)
            return;

        UpdatePortalCamera();
    }

    public void SetMainEntranceData(MainEntranceData mainEntrance)
    {
        _mainEntrance = mainEntrance;

        HDAdditionalCameraData additionalCameraData = _portalCamera.GetComponent<HDAdditionalCameraData>();

        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
        {
            additionalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
        }
        else
        {
            additionalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        }
    }

    public void LinkPortal(DoorPortal other)
    {
        _linkedPortal = other;

        SetScreenRenderTexture(other._viewTexture);

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked portal {_mainEntrance.EntranceTeleport.GetLogInfo()} -> {other._mainEntrance.EntranceTeleport.GetLogInfo()}");
    }

    #region Drawing
    private void SetScreenRenderTexture(RenderTexture renderTexture)
    {
        _screen.material.SetTexture("_MainTex", renderTexture);
    }

    private void SetDrawing(bool value)
    {
        _isDrawing = value;

        _screen.gameObject.SetActive(value);

        if (_mainEntrance == null)
            return;

        if (_mainEntrance.HasViewBlocker)
        {
            if (IsRendering())
            {
                _mainEntrance.ViewBlockerObject.SetActive(false);
            }
            else
            {
                _mainEntrance.ViewBlockerObject.SetActive(!value);
            }
        }

        // Commented this out for debugging to have a clean view. Add this back in later.
        //_mainEntrance.SetDoorObjectsEnabled(!IsRendering());

        _mainEntrance.SetDoorObjectsEnabled(false);
    }
    #endregion

    #region Rendering
    private void CreateViewTexture()
    {
        _viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
        _portalCamera.targetTexture = _viewTexture;
    }

    private bool IsRendering()
    {
        return _renderingContainer.activeSelf && _portalCamera.enabled;
    }

    private void SetRendering(bool value)
    {
        _renderingContainer.SetActive(value);
        _portalCamera.enabled = true;
    }

    private void UpdatePortalCamera()
    {
        if (!IsRendering())
            return;

        Camera playerCamera = GetLocalPlayerCamera();
        if (playerCamera == null) return;

        Transform linkedScreen = _linkedPortal._screen.transform;
        Transform thisScreen = _screen.transform;

        // --- 1. Position & Rotation (unchanged, already correct) ---
        Matrix4x4 linkedFlipped = linkedScreen.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 180f, 0f));

        Matrix4x4 relativeTransform = thisScreen.localToWorldMatrix * Matrix4x4.Inverse(linkedFlipped);

        _portalCamera.transform.SetPositionAndRotation(
            relativeTransform.MultiplyPoint(playerCamera.transform.position),
            relativeTransform.rotation * playerCamera.transform.rotation
        );

        // --- 2. Oblique Near-Clip Projection ---
        SetObliqueNearClipPlane(playerCamera);
    }

    private void SetObliqueNearClipPlane(Camera playerCamera)
    {
        Transform screenTransform = _screen.transform;

        Plane clipPlane = new Plane(-screenTransform.forward, screenTransform.position);

        Vector4 clipPlaneVec = new Vector4(
            clipPlane.normal.x,
            clipPlane.normal.y,
            clipPlane.normal.z,
            clipPlane.distance
        );

        Vector4 clipPlaneCameraSpace =
            Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) * clipPlaneVec;

        _portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
    #endregion

    #region Player Camera
    private bool IsLocalPlayerCameraNearby()
    {
        if (!TryGetLocalPlayerCamera(out Camera playerCamera))
            return false;

        Vector3 cameraPosition = playerCamera.transform.position;

        float distance = Vector3.Distance(cameraPosition, transform.position);

        return distance <= 10f;
    }

    private static Camera GetLocalPlayerCamera()
    {
        PlayerControllerB playerScript = PlayerUtils.LocalPlayerScript;

        if (playerScript == null || playerScript.isPlayerDead)
        {
            return StartOfRound.Instance.spectateCamera;
        }

        return playerScript.gameplayCamera;
    }

    private static bool TryGetLocalPlayerCamera(out Camera camera)
    {
        camera = GetLocalPlayerCamera();
        return camera != null;
    }
    #endregion
}
