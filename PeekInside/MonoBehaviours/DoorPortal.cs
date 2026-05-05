using com.github.zehsteam.PeekInside.Extensions;
using com.github.zehsteam.PeekInside.Helpers;
using com.github.zehsteam.PeekInside.Managers;
using com.github.zehsteam.PeekInside.Objects;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    private static readonly List<DoorPortal> _instances = [];

    public Camera PortalCamera => _portalCamera;

    #region Unity Editor
    [SerializeField]
    private Transform _pivot;

    [SerializeField]
    private MeshRenderer _screen;

    [SerializeField]
    private MeshRenderer _screenOccluder;

    [SerializeField]
    private GameObject _renderingContainer;

    [SerializeField]
    private Camera _portalCamera;

    [SerializeField]
    private Light _nightVision;

    [SerializeField]
    private Transform _volumeContainer;

    [Space(10f)]
    [SerializeField]
    private List<DoorPortalMoonSettings> _moonSettingsList = [];

    [Space(10f)]
    [SerializeField]
    private List<DoorPortalInteriorSettings> _interiorSettingsList = [];
    #endregion

    private MainEntranceData _mainEntrance;
    private DoorPortal _linkedPortal;
    private RenderTexture _viewTexture;
    private bool _isDrawing;
    private bool _isInRange;

    private LayerMask _pivotRaycastMask;

    private void Awake()
    {
        _pivotRaycastMask = LayerMask.GetMask("Room");

        CreateViewTexture();

        SetDrawing(false);
        SetRendering(false);
    }

    private void OnEnable()
    {
        if (!_instances.Contains(this))
        {
            _instances.Add(this);
        }
    }

    private void OnDisable()
    {
        _instances.Remove(this);
    }

    private void Start()
    {
        InitializeVolumes();
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

        InitializePivot();
        ApplyScreenCrop();
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
        bool isScreenVisible = CameraHelper.IsVisibleFromCamera(_screen, PlayerUtils.GetLocalPlayerCamera());

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

    private bool IsLocalPlayerCameraInRange()
    {
        if (!PlayerUtils.TryGetLocalPlayerCamera(out Camera playerCamera))
            return false;

        float range = ConfigManager.Portal_ActivationRange.Value;

        Vector3 cameraPosition = playerCamera.transform.position;
        float distance = Vector3.Distance(cameraPosition, transform.position);

        return distance <= range;
    }

    private void OnLocalPlayerEnterRange()
    {
        SetDrawing(true);
        _linkedPortal.SetRendering(true);

        if (_mainEntrance.IsOutside)
        {
            InteriorHelper.RenderInterior();
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

        }
        else
        {
            OutsideHelper.SetSunEnabled(false);
        }
    }

    #region Settings
    private DoorPortalMoonSettings GetMoonSettings()
    {
        return _moonSettingsList.FirstOrDefault(x => x.PlanetName == LevelHelper.GetCurrentMoonName());
    }

    private bool TryGetMoonSettings(out DoorPortalMoonSettings moonSettings)
    {
        moonSettings = GetMoonSettings();
        return moonSettings != null;
    }

    private DoorPortalInteriorSettings GetInteriorSettings()
    {
        return _interiorSettingsList.FirstOrDefault(x => x.InteriorType == InteriorHelper.GetCurrentInteriorType());
    }

    private bool TryGetInteriorSettings(out DoorPortalInteriorSettings interiorSettings)
    {
        interiorSettings = GetInteriorSettings();
        return interiorSettings != null;
    }
    #endregion

    #region Volumes
    private void InitializeVolumes()
    {
        CreateVolumesFromScene();
    }

    private void CreateVolumesFromScene()
    {
        foreach (var child in _volumeContainer.GetChildren())
        {
            Destroy(child.gameObject);
        }

        Volume[] volumes = [.. FindObjectsByType<Volume>(FindObjectsSortMode.None)
            .Where(x => x.isGlobal && x.gameObject.layer == 0)];

        if (volumes.Length == 0)
            return;

        LayerMask volumeLayer = LayerMask.NameToLayer("NavigationSurface");

        foreach (var volume in volumes)
        {
            GameObject obj = Instantiate(volume.gameObject, _volumeContainer);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            obj.layer = volumeLayer;

            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.size = Vector3.one * 0.1f;

            Volume newVolume = obj.GetComponent<Volume>();
            newVolume.isGlobal = false;
        }
    }
    #endregion

    #region Pivot
    private void InitializePivot()
    {
        if (_mainEntrance == null)
            return;

        if (_mainEntrance.IsOutside)
        {
            SetDynamicPivot();
            return;
        }

        if (TryGetInteriorSettings(out DoorPortalInteriorSettings interiorSettings))
        {
            if (interiorSettings.UseDynamicPivot)
            {
                SetDynamicPivot();
            }

            _pivot.localPosition += interiorSettings.PivotPositionOffset;
        }
        else
        {
            SetDynamicPivot();
        }
    }

    private void SetDynamicPivot()
    {
        if (TryGetDynamicPivotPosition(out Vector3 position))
        {
            _pivot.position = position;
        }

        if (TryGetDynamicPivotRotation(out Quaternion rotation))
        {
            _pivot.rotation = rotation;
        }
    }

    private bool TryGetDynamicPivotPosition(out Vector3 position)
    {
        position = Vector3.zero;

        Vector3 origin = _pivot.position + _pivot.forward * -0.25f;

        if (!TryRaycastForPivot(origin, _pivot.forward, out RaycastHit hitForward))
            return false;

        float offsetFromWall = -0.001f;

        Vector3 newPosition = hitForward.point + _pivot.forward * offsetFromWall;

        if (TryRaycastForPivot(origin, -_pivot.up, out RaycastHit hitDown, maxDistance: 5f))
        {
            float yOffset = _screen.transform.lossyScale.y / 2f;
            float yPosition = hitDown.point.y + yOffset;

            newPosition.y = yPosition;
        }

        position = newPosition;
        return true;
    }

    private bool TryGetDynamicPivotRotation(out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        Vector3 origin = _pivot.position + _pivot.forward * -0.25f;
        Vector3 direction = _pivot.forward;

        Vector3 leftOrigin = origin + _pivot.right * -0.25f;
        bool didLeftHit = TryRaycastForPivot(leftOrigin, direction, out RaycastHit leftHit);

        if (!didLeftHit)
            return false;

        Vector3 rightOrigin = origin + _pivot.right * 0.25f;
        bool didRightHit = TryRaycastForPivot(rightOrigin, direction, out RaycastHit rightHit);

        if (!didRightHit)
            return false;

        Vector3 averageNormal = (leftHit.normal + rightHit.normal).normalized;
        Vector3 surfaceRight = (rightHit.point - leftHit.point).normalized;
        Vector3 surfaceForward = -averageNormal;
        Vector3 surfaceUp = Vector3.Cross(surfaceForward, surfaceRight).normalized;

        rotation = Quaternion.LookRotation(surfaceForward, surfaceUp);
        return true;
    }
    
    private bool TryRaycastForPivot(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance = 1f)
    {
        return Physics.Raycast(origin, direction, out hit, maxDistance, _pivotRaycastMask, QueryTriggerInteraction.Ignore);
    }
    #endregion

    #region Drawing
    private void ApplyScreenCrop()
    {
        if (_mainEntrance == null)
            return;

        if (_mainEntrance.IsOutside)
            return;

        if (TryGetInteriorSettings(out DoorPortalInteriorSettings interiorSettings))
        {
            float cropLeft = interiorSettings.ScreenCropLeft;
            float cropRight = interiorSettings.ScreenCropRight;
            float cropTop = interiorSettings.ScreenCropTop;
            float cropBottom = interiorSettings.ScreenCropBottom;

            SetScreenCrop(cropLeft, cropRight, cropTop, cropBottom);
        }
    }

    private void SetScreenCrop(float left, float right, float top, float bottom)
    {
        _screen.material.SetFloat("_CropLeft", left);
        _screen.material.SetFloat("_CropRight", right);
        _screen.material.SetFloat("_CropTop", top);
        _screen.material.SetFloat("_CropBottom", bottom);

        float xScale = 1f - left - right;
        float yScale = 1f - top - bottom;

        _screenOccluder.transform.localScale = new Vector3(xScale, yScale, 1f);

        // Offset is in the screen's local space (-0.5 to 0.5 range)
        // Move toward right when right is cropped less, toward left when left is cropped less
        float xOffset = (left - right) * 0.5f;
        float yOffset = (bottom - top) * 0.5f;

        Vector3 previousOccluderPosition = _screenOccluder.transform.localPosition;

        _screenOccluder.transform.localPosition = new Vector3(xOffset, yOffset, previousOccluderPosition.z);
    }

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
    private void ApplyCameraViewRange()
    {
        if (_mainEntrance == null)
            return;

        float farClipPlane;

        if (_mainEntrance.IsOutside)
        {
            float outsideViewRange = ConfigManager.Portal_OutsideViewRange.Value;

            if (TryGetMoonSettings(out DoorPortalMoonSettings moonSettings) && moonSettings.UseViewRange)
            {
                farClipPlane = Mathf.Min(moonSettings.ViewRange, outsideViewRange);
            }
            else
            {
                farClipPlane = outsideViewRange;
            }
        }
        else
        {
            farClipPlane = ConfigManager.Portal_InsideViewRange.Value;
        }

        _portalCamera.farClipPlane = farClipPlane;
    }

    private void CreateViewTexture()
    {
        Size targetScreenSize = PlayerUtils.GetCameraRenderTextureSize();

        bool CanCreate()
        {
            if (_viewTexture == null)
                return true;

            if (_viewTexture.width != targetScreenSize.Width)
                return true;

            if (_viewTexture.height != targetScreenSize.Height)
                return true;

            return false;
        }

        if (!CanCreate())
            return;

        _viewTexture?.Release();
        _viewTexture = new RenderTexture(targetScreenSize.Width, targetScreenSize.Height, 24, RenderTextureFormat.DefaultHDR);

        Logger.LogInfo($"[{nameof(DoorPortal)}] {nameof(CreateViewTexture)}() width: {targetScreenSize.Width}, height: {targetScreenSize.Height}", extended: true);

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

        if (!PlayerUtils.TryGetLocalPlayerCamera(out Camera playerCamera))
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

    public static bool TryGetRenderingInstance(out DoorPortal doorPortal)
    {
        doorPortal = _instances.FirstOrDefault(x => x.IsRendering());
        return doorPortal != null;
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

    private void ApplyConfigSettings()
    {
        ApplyCameraViewRange();
    }

    public static void OnConfigSettingsChanged()
    {
        foreach (var instance in _instances)
        {
            instance.ApplyConfigSettings();
        }
    }
}
