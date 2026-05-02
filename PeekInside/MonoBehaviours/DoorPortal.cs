using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using com.github.zehsteam.PeekInside.Objects;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    #region Unity Editor
    [SerializeField]
    private MeshRenderer _screen;

    [SerializeField]
    private Camera _portalCamera;

    [SerializeField]
    private GameObject _renderingContainer;

    [SerializeField]
    private Light _nightVision;
    #endregion

    private MainEntranceData _mainEntrance;
    private DoorPortal _linkedPortal;
    private RenderTexture _viewTexture;
    private bool _isDrawing;
    private bool _isInRange;

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

        bool enabled = ConfigManager.DoorPortals_Enabled.Value;

        if (!enabled)
        {
            if (_isDrawing)
            {
                SetDrawing(false);
            }

            return;
        }

        if (IsLocalPlayerCameraNearby())
        {
            if (_isInRange) return;
            _isInRange = true;

            OnEnterRange();
        }
        else
        {
            if (!_isInRange) return;
            _isInRange = false;

            OnExitRange();
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
    
    private void OnEnterRange()
    {
        SetDrawing(true);
        _linkedPortal.SetRendering(true);

        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
        {
            FacilityHelper.RenderFacility();
            FacilityHelper.SetFogEnabled(true);
        }
    }

    private void OnExitRange()
    {
        SetDrawing(false);
        _linkedPortal.SetRendering(false);

        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
        {
            FacilityHelper.SetFogEnabled(false);
        }
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

        UpdateDoor();
    }
    #endregion

    #region Rendering
    private void CreateViewTexture()
    {
        bool CanCreate()
        {
            if (_viewTexture == null)
                return true;

            if (_viewTexture.width != Screen.width)
                return true;

            if (_viewTexture.height != Screen.height)
                return true;

            return false;
        }

        if (!CanCreate())
            return;

        _viewTexture?.Release();
        _viewTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);

        _portalCamera.targetTexture = _viewTexture;

        _linkedPortal?.SetScreenRenderTexture(_viewTexture);
    }

    private bool IsRendering()
    {
        return _renderingContainer.activeSelf;
    }

    private void SetRendering(bool value)
    {
        _renderingContainer.SetActive(value);

        if (_mainEntrance == null)
            return;

        UpdateNightVision();
        UpdateDoor();
    }

    private void UpdateNightVision()
    {
        if (_mainEntrance == null)
            return;

        if (_mainEntrance.EntranceTeleport.isEntranceToBuilding)
            return;

        _nightVision.enabled = IsRendering();
    }

    private void UpdatePortalCamera()
    {
        if (!IsRendering())
            return;

        if (!TryGetLocalPlayerCamera(out Camera playerCamera))
            return;

        CreateViewTexture(); // Will create a new render texture if the screen size has changed

        Transform linkedScreen = _linkedPortal._screen.transform;
        Transform thisScreen = _screen.transform;

        Matrix4x4 linkedFlipped = linkedScreen.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 180f, 0f));

        Matrix4x4 relativeTransform = thisScreen.localToWorldMatrix * Matrix4x4.Inverse(linkedFlipped);

        _portalCamera.transform.SetPositionAndRotation(
            relativeTransform.MultiplyPoint(playerCamera.transform.position),
            relativeTransform.rotation * playerCamera.transform.rotation
        );

        SetObliqueNearClipPlane(playerCamera);
    }

    private void SetObliqueNearClipPlane(Camera playerCamera)
    {
        Transform screenTransform = _screen.transform;

        var clipPlane = new Plane(-screenTransform.forward, screenTransform.position);

        var clipPlaneVec = new Vector4(
            clipPlane.normal.x,
            clipPlane.normal.y,
            clipPlane.normal.z,
            clipPlane.distance
        );

        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(_portalCamera.worldToCameraMatrix)) * clipPlaneVec;

        _portalCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
    #endregion

    private void UpdateDoor()
    {
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
                _mainEntrance.ViewBlockerObject.SetActive(!_isDrawing);
            }
        }

        bool hideDoorObjects = ConfigManager.Debug_HideDoorObjects.Value;

        if (hideDoorObjects)
        {
            _mainEntrance.SetDoorObjectsEnabled(false);
        }
        else
        {
            _mainEntrance.SetDoorObjectsEnabled(!IsRendering());
        }
    }

    #region Player Camera
    private bool IsLocalPlayerCameraNearby()
    {
        if (!TryGetLocalPlayerCamera(out Camera playerCamera))
            return false;

        Vector3 cameraPosition = playerCamera.transform.position;

        float distance = Vector3.Distance(cameraPosition, transform.position);

        float range = ConfigManager.DoorPortals_ActiveRange.Value;

        return distance <= range;
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
