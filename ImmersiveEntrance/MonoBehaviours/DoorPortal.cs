using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;

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
    #endregion

    private MainEntranceData _mainEntrance;
    private PortalSettings _portalSettings;
    private DoorPortal _linkedPortal;
    
    private RenderTexture _viewTexture;
    private bool _isDrawing;
    private bool _isInRange;
    private LayerMask _pivotRaycastMask;

    private void Awake()
    {
        _pivotRaycastMask = LayerMask.GetMask("Room");

        _portalCamera.enabled = false;

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
        InitializePivot();
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

        if (mainEntrance.IsOutside)
        {
            _portalSettings = PortalSettingsManager.GetCurrentMoonSettings();
        }
        else
        {
            _portalSettings = PortalSettingsManager.GetCurrentInteriorSettings();
        }
        
        InitializeScreen();
        InitializeCamera();
    }

    public void LinkPortal(DoorPortal other)
    {
        _linkedPortal = other;

        SetScreenRenderTexture(other._viewTexture);

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked portal {_mainEntrance.EntranceTeleport.GetLogInfo()} -> {other._mainEntrance.EntranceTeleport.GetLogInfo()}");
    }

    public bool IsEnabled()
    {
        if (!ConfigManager.Portal_Enabled.Value)
            return false;

        if (_portalSettings == null || _linkedPortal == null)
            return false;

        if (!_portalSettings.Enabled.Value)
            return false;

        if (!_linkedPortal._portalSettings.Enabled.Value)
            return false;

        return true;
    }

    private void UpdateScreenVisibility()
    {
        if (!IsEnabled())
        {
            _isInRange = false;
            OnLocalPlayerExitRange();
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
            //OutsideHelper.SetSunEnabled(true);
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
            //OutsideHelper.SetSunEnabled(false);
        }
    }

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
        if (_portalSettings.UseDynamicPivot)
        {
            SetDynamicPivot();
        }

        _pivot.localPosition += _portalSettings.PivotPositionOffset;
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
    private void InitializeScreen()
    {
        ApplyScreenCrop();
    }

    private void ApplyScreenCrop()
    {
        SetScreenCrop(_portalSettings.ScreenCrop);
    }

    private void SetScreenCrop(Padding padding)
    {
        _screen.material.SetFloat("_CropLeft", padding.Left);
        _screen.material.SetFloat("_CropRight", padding.Right);
        _screen.material.SetFloat("_CropTop", padding.Top);
        _screen.material.SetFloat("_CropBottom", padding.Bottom);

        float xScale = 1f - padding.Left - padding.Right;
        float yScale = 1f - padding.Top - padding.Bottom;

        _screenOccluder.transform.localScale = new Vector3(xScale, yScale, 1f);

        // Offset is in the screen's local space (-0.5 to 0.5 range)
        // Move toward right when right is cropped less, toward left when left is cropped less
        float xOffset = (padding.Left - padding.Right) * 0.5f;
        float yOffset = (padding.Bottom - padding.Top) * 0.5f;

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
    private void InitializeCamera()
    {
        if (_mainEntrance == null)
            return;

        HDAdditionalCameraData portalCameraData = _portalCamera.GetComponent<HDAdditionalCameraData>();

        if (_mainEntrance.IsOutside)
        {
            portalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
        }
        else
        {
            portalCameraData.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        }

        CreateViewTexture();
    }

    private void ApplyCameraViewDistance()
    {
        if (_mainEntrance == null)
            return;

        float farClipPlane;

        if (_portalSettings.UseViewDistance.Value)
        {
            farClipPlane = _portalSettings.ViewDistance.Value;
        }
        else
        {
            if (_mainEntrance.IsOutside)
            {
                farClipPlane = ConfigManager.Portal_OutsideViewDistance.Value;
            }
            else
            {
                farClipPlane = ConfigManager.Portal_InsideViewDistance.Value;
            }
        }

        _portalCamera.farClipPlane = farClipPlane;
    }

    private Size GetTargetScreenSize()
    {
        PixelResolutionType pixelResolution = ConfigManager.Portal_PixelResolution.Value;

        return pixelResolution switch
        {
            PixelResolutionType.PlayerCamera => CameraHelper.GetCameraScreenSize(),
            PixelResolutionType.Default =>          new Size(860, 520),
            PixelResolutionType.Performance =>      new Size(620, 364),
            PixelResolutionType.UltraPerformance => new Size(400, 260),
            PixelResolutionType.Retro =>            new Size(186, 104),
            _ => CameraHelper.GetCameraScreenSize(),
        };
    }

    private void CreateViewTexture()
    {
        Size targetScreenSize = GetTargetScreenSize();

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

        if (value)
        {
            ApplyCameraViewDistance();
        }

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

        OutsideHelper.SetSunEnabled(_mainEntrance.IsOutside);

        _portalCamera.Render();

        OutsideHelper.SetSunEnabled(!_mainEntrance.IsOutside);
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

    #region Config
    private void ApplyConfigSettings()
    {
        ApplyCameraViewDistance();
    }

    public static void OnConfigSettingsChanged()
    {
        foreach (var instance in _instances)
        {
            instance.ApplyConfigSettings();
        }
    }
    #endregion
}
