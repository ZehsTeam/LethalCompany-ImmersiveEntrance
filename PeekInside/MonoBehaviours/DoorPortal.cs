using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using com.github.zehsteam.PeekInside.Objects;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Linq;
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
    private Transform _pivot;

    [SerializeField]
    private GameObject _renderingContainer;

    [SerializeField]
    private Light _nightVision;

    [Space(10f)]
    [SerializeField]
    private List<InteriorDoorPortalSettings> _interiorSettingsList = [];
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

        UpdateScreenVisibility();
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

        HDAdditionalCameraData portalCameraData = _portalCamera.GetComponent<HDAdditionalCameraData>();

        if (_mainEntrance.IsOutside)
        {
            portalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
        }
        else
        {
            portalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        }

        SetPivot();
        ApplyConfigSettings();
    }

    public void LinkPortal(DoorPortal other)
    {
        _linkedPortal = other;

        SetScreenRenderTexture(other._viewTexture);

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked portal {_mainEntrance.EntranceTeleport.GetLogInfo()} -> {other._mainEntrance.EntranceTeleport.GetLogInfo()}");
    }

    private void UpdateScreenVisibility()
    {
        bool enabled = ConfigManager.Portal_Enabled.Value;

        if (!enabled)
        {
            if (_isDrawing)
            {
                SetDrawing(false);
            }

            return;
        }

        bool inRange = IsLocalPlayerCameraInRange();
        bool isScreenVisible = Utils.IsVisibleFromCamera(_screen, GetLocalPlayerCamera());

        if (inRange && isScreenVisible)
        {
            if (_isInRange) return;
            _isInRange = true;

            OnLocalPlayerEnterRange();
        }
        else
        {
            if (!_isInRange) return;
            _isInRange = false;

            OnLocalPlayerExitRange();
        }
    }

    private void OnLocalPlayerEnterRange()
    {
        SetDrawing(true);
        _linkedPortal.SetRendering(true);

        if (_mainEntrance.IsOutside)
        {
            InteriorHelper.RenderInterior();
            InteriorHelper.SetFogEnabled(true);
        }
        else
        {
            OutsideHelper.SetSunEnabled(true);
        }
    }

    private void OnLocalPlayerExitRange()
    {
        SetDrawing(false);
        _linkedPortal.SetRendering(false);

        if (_mainEntrance.IsOutside)
        {
            InteriorHelper.SetFogEnabled(false);
        }
        else
        {
            OutsideHelper.SetSunEnabled(false);
        }
    }

    #region Pivot
    private void SetPivot()
    {
        if (_mainEntrance == null)
            return;

        if (_mainEntrance.IsOutside)
        {
            SetDynamicPivotPositionAndRotation();
            return;
        }

        InteriorDoorPortalSettings interiorSettings = GetInteriorSettings();

        if (interiorSettings == null)
        {
            SetDynamicPivotPositionAndRotation();
            return;
        }

        if (interiorSettings.UseDynamicPivot)
        {
            SetDynamicPivotPositionAndRotation();
        }

        _pivot.localPosition += interiorSettings.PivotPositionOffset;
    }

    private void SetDynamicPivotPositionAndRotation()
    {
        Vector3 middleOrigin = transform.position;
        Vector3 direction = _pivot.forward;

        // Position

        bool didMiddleHit = TryRaycastForPivot(middleOrigin, direction, out RaycastHit middleHit);

        if (!didMiddleHit)
            return;

        float offsetFromWall = -0.001f;

        Vector3 newPosition = middleHit.point + _pivot.forward * offsetFromWall;
        _pivot.position = newPosition;

        // Rotation

        Vector3 leftOrigin = middleOrigin + _pivot.right * -0.25f;
        bool didLeftHit = TryRaycastForPivot(leftOrigin, direction, out RaycastHit leftHit);

        if (!didLeftHit)
            return;

        Vector3 rightOrigin = middleOrigin + _pivot.right * 0.25f;
        bool didRightHit = TryRaycastForPivot(rightOrigin, direction, out RaycastHit rightHit);

        if (!didRightHit)
            return;

        Vector3 averageNormal = (leftHit.normal + rightHit.normal).normalized;
        Vector3 surfaceRight = (rightHit.point - leftHit.point).normalized;
        Vector3 surfaceForward = -averageNormal;
        Vector3 surfaceUp = Vector3.Cross(surfaceForward, surfaceRight).normalized;

        _pivot.rotation = Quaternion.LookRotation(surfaceForward, surfaceUp);
    }

    private bool TryRaycastForPivot(Vector3 origin, Vector3 direction, out RaycastHit hit)
    {
        float maxDistance = 1f;
        LayerMask layerMask = LayerMask.GetMask("Room");

        return Physics.Raycast(origin, direction, out hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
    }

    private InteriorDoorPortalSettings GetInteriorSettings()
    {
        return _interiorSettingsList.FirstOrDefault(x => x.InteriorType == InteriorHelper.GetCurrentInteriorType());
    }
    #endregion

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

        if (_mainEntrance.IsOutside)
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

        if (_mainEntrance.HasDoorViewBlocker)
        {
            if (IsRendering())
            {
                _mainEntrance.DoorViewBlocker.SetActive(false);
            }
            else
            {
                _mainEntrance.DoorViewBlocker.SetActive(!_isDrawing);
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
    private bool IsLocalPlayerCameraInRange()
    {
        if (!TryGetLocalPlayerCamera(out Camera playerCamera))
            return false;

        float range = ConfigManager.Portal_ActivationRange.Value;

        Vector3 cameraPosition = playerCamera.transform.position;
        float distance = Vector3.Distance(cameraPosition, transform.position);

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

    private void ApplyConfigSettings()
    {
        if (_mainEntrance == null)
            return;

        float farClipPlane;

        if (_mainEntrance.IsOutside)
        {
            farClipPlane = ConfigManager.Portal_OutsideViewRange.Value;
        }
        else
        {
            farClipPlane = ConfigManager.Portal_InsideViewRange.Value;
        }

        _portalCamera.farClipPlane = farClipPlane;
    }

    public static void OnConfigSettingsChanged()
    {
        EntranceManager.OutsideMainEntrance?.DoorPortal?.ApplyConfigSettings();
        EntranceManager.InsideMainEntrance?.DoorPortal?.ApplyConfigSettings();
    }
}
