using com.github.zehsteam.ImmersiveEntrance.Dependencies.CullFactoryMod;
using com.github.zehsteam.ImmersiveEntrance.Extensions;
using com.github.zehsteam.ImmersiveEntrance.Helpers;
using com.github.zehsteam.ImmersiveEntrance.Managers;
using com.github.zehsteam.ImmersiveEntrance.Objects;
using com.github.zehsteam.ImmersiveEntrance.Objects.PortalSettingTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    private static readonly List<DoorPortal> _instances = [];

    #region Unity Editor
    #pragma warning disable CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
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
    private GameObject _fogExclusionZone;
    #pragma warning restore CS0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    #endregion

    public Camera PortalCamera => _portalCamera;
    public bool IsOutside => MainEntrance.IsOutside;

    public MainEntranceData MainEntrance { get; private set; }
    public PortalSettings PortalSettings { get; private set; }
    public DoorPortal LinkedPortal { get; private set; }
    
    private RenderTexture _viewTexture;
    private bool _isDrawing;
    private bool _isLocalPlayerInRange;
    private LayerMask _pivotRaycastMask;

    private void Awake()
    {
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

    private void Update()
    {
        if (LinkedPortal == null)
            return;

        UpdateScreenVisibility();
    }

    private void LateUpdate()
    {
        if (LinkedPortal == null)
            return;

        UpdatePortalCamera();
    }

    public void SetMainEntranceData(MainEntranceData mainEntrance)
    {
        MainEntrance = mainEntrance;

        if (mainEntrance == null)
        {
            Logger.LogError($"[{nameof(DoorPortal)}] {nameof(SetMainEntranceData)}: MainEntranceData is null!");
            return;
        }

        if (mainEntrance.IsOutside)
        {
            PortalSettings = PortalSettingsManager.MoonDatabase.GetEntryForCurrentMoon();
        }
        else
        {
            PortalSettings = PortalSettingsManager.InteriorDatabase.GetEntryForCurrentInterior();
        }

        if (PortalSettings == null)
        {
            Logger.LogError($"[{nameof(DoorPortal)}] {nameof(SetMainEntranceData)}: PortalSettings is null!");
            return;
        }

        Utils.InvokeAfterDelay(InitializePivot, TimeSpan.FromSeconds(0.1f));

        InitializeScreen();
        InitializeCamera();

        ApplyConfigSettings();
    }

    public void LinkPortal(DoorPortal other)
    {
        LinkedPortal = other;

        SetScreenRenderTexture(other._viewTexture);

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked portal {MainEntrance.EntranceTeleport.GetLogInfo()} -> {other.MainEntrance.EntranceTeleport.GetLogInfo()}");
    }

    public bool IsEnabled()
    {
        if (!ConfigManager.Portal_Enabled.Value)
            return false;

        if (PortalSettings == null || LinkedPortal == null)
            return false;

        if (!PortalSettings.Enabled.Value)
            return false;

        if (!LinkedPortal.PortalSettings.Enabled.Value)
            return false;

        return true;
    }

    #region Screen Visibility
    private void UpdateScreenVisibility()
    {
        if (!IsEnabled())
        {
            _isLocalPlayerInRange = false;
            OnLocalPlayerExitRange();
            return;
        }

        bool inRange = IsLocalPlayerCameraInRange();
        bool isScreenVisible = CameraHelper.IsVisibleFromCamera(_screen, PlayerUtils.GetLocalPlayerCamera());

        if (inRange && isScreenVisible)
        {
            if (_isLocalPlayerInRange) return;
            _isLocalPlayerInRange = true;

            OnLocalPlayerEnterRange();
        }
        else
        {
            if (!_isLocalPlayerInRange) return;
            _isLocalPlayerInRange = false;

            OnLocalPlayerExitRange();
        }
    }

    private bool IsLocalPlayerCameraInRange()
    {
        if (!PlayerUtils.TryGetLocalPlayerCamera(out Camera playerCamera))
            return false;

        float range = ConfigManager.Portal_ActivationRange.Value;

        Vector3 cameraPosition = playerCamera.transform.position;
        float distance = Vector3.Distance(cameraPosition, _pivot.position);

        return distance <= range;
    }

    private void OnLocalPlayerEnterRange()
    {
        SetDrawing(true);
        LinkedPortal.SetRendering(true);

        if (MainEntrance.IsOutside)
        {
            if (CullFactoryProxy.IsInstalled)
            {
                CullFactoryProxy.DisableCullFactory();
            }

            InteriorHelper.RenderInterior();
        }
        else
        {
            LevelHelper.SetForceWeatherEffectsEnabled(true);
        }
    }

    private void OnLocalPlayerExitRange()
    {
        SetDrawing(false);
        LinkedPortal.SetRendering(false);

        if (MainEntrance.IsOutside)
        {
            if (CullFactoryProxy.IsInstalled)
            {
                CullFactoryProxy.EnableCullFactory();
            }
        }
        else
        {
            LevelHelper.SetForceWeatherEffectsEnabled(false);
        }
    }
    #endregion

    #region Pivot
    private void InitializePivot()
    {
        InitializePivot(PortalSettings.UseDynamicPivot, PortalSettings.PivotPositionOffset, PortalSettings.PivotRotationOffset);
    }

    public void InitializePivot(bool useDynamicPivot, Vector3 positionOffset, Vector3 rotationOffset)
    {
        _pivot.localPosition = Vector3.zero;
        _pivot.localRotation = Quaternion.identity;

        if (useDynamicPivot)
        {
            SetDynamicPivot();
        }

        _pivot.localPosition += positionOffset;
        _pivot.localRotation = Quaternion.Euler(_pivot.localRotation.eulerAngles + rotationOffset);
    }

    private void SetDynamicPivot()
    {
        _pivotRaycastMask = LayerMask.GetMask("Room", "Colliders");

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

        float offsetFromWall = -0.0015f;

        Vector3 newPosition = hitForward.point + _pivot.forward * offsetFromWall;

        if (TryRaycastForPivot(newPosition, -_pivot.up, out RaycastHit hitDown, maxDistance: 5f))
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
        SetScreenCrop(PortalSettings.ScreenCrop);
    }

    // This is here to enable testing this in UnityExplorer
    public void SetScreenCrop(float left, float right, float top, float bottom)
    {
        SetScreenCrop(new Padding(left, right, top, bottom));
    }

    public void SetScreenCrop(Padding padding, bool isUnityEditor = false)
    {
        void SetMaterialFloat(string name, float value)
        {
            if (isUnityEditor)
            {
                //_screen.sharedMaterial.SetFloat(name, value);
            }
            else
            {
                _screen.material.SetFloat(name, value);
            }
        }

        SetMaterialFloat("_CropLeft", padding.Left);
        SetMaterialFloat("_CropRight", padding.Right);
        SetMaterialFloat("_CropTop", padding.Top);
        SetMaterialFloat("_CropBottom", padding.Bottom);

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
        CreateViewTexture();
    }

    private void ApplyCameraConfigSettings()
    {
        bool fogEnabled = ConfigManager.PortalGraphics_FogEnabled.Value;
        bool customPassEnabled = ConfigManager.PortalGraphics_CustomPassEnabled.Value;

        HDAdditionalCameraData portalCameraData = _portalCamera.GetComponent<HDAdditionalCameraData>();
        portalCameraData.customRenderingSettings = true;

        FrameSettings settings = portalCameraData.renderingPathCustomFrameSettings;

        settings.SetEnabled(FrameSettingsField.AtmosphericScattering, fogEnabled);
        settings.SetEnabled(FrameSettingsField.CustomPass, customPassEnabled);

        portalCameraData.renderingPathCustomFrameSettings = settings;
    }

    private Size GetTargetViewTextureSize()
    {
        PixelResolutionType pixelResolution = ConfigManager.PortalGraphics_PixelResolution.Value;

        if (pixelResolution == PixelResolutionType.PlayerCamera)
            return CameraHelper.GetCameraScreenSize();

        Size targetSize = pixelResolution switch
        {
            PixelResolutionType.Default => new Size(860, 520),
            PixelResolutionType.Performance => new Size(620, 364),
            PixelResolutionType.UltraPerformance => new Size(400, 260),
            PixelResolutionType.Retro => new Size(186, 104),
            _ => CameraHelper.GetCameraScreenSize(),
        };

        Size playerCameraSize = CameraHelper.GetCameraScreenSize();

        if (targetSize.Width > playerCameraSize.Width)
            targetSize.Width = playerCameraSize.Width;

        if (targetSize.Height > playerCameraSize.Height)
            targetSize.Height = playerCameraSize.Height;

        return targetSize;
    }

    private void CreateViewTexture()
    {
        Size targetSize = GetTargetViewTextureSize();

        bool CanCreate()
        {
            if (_viewTexture == null)
                return true;

            if (_viewTexture.width != targetSize.Width)
                return true;

            if (_viewTexture.height != targetSize.Height)
                return true;

            return false;
        }

        if (!CanCreate())
            return;

        _viewTexture?.Release();

        var descriptor = new RenderTextureDescriptor(targetSize.Width, targetSize.Height)
        {
            colorFormat = RenderTextureFormat.ARGB32,        // R8G8B8A8_SRGB equivalent
            depthStencilFormat = GraphicsFormat.D32_SFloat,  // D32_SFLOAT
            depthBufferBits = 32,
            msaaSamples = 1,                                 // Anti-aliasing off
            mipCount = 0,                                    // Mip maps off
            useMipMap = false,                               // Mip maps off
            autoGenerateMips = false,
            enableRandomWrite = false,                       // Random write off
            useDynamicScale = false,                         // Dynamic scaling off
            sRGB = true,                                     // SRGB on for R8G8B8A8_SRGB
        };

        _viewTexture = new RenderTexture(descriptor)
        {
            filterMode = FilterMode.Point
        };

        _viewTexture.Create();

        Logger.LogInfo($"[{nameof(DoorPortal)}] {nameof(CreateViewTexture)}() width: {targetSize.Width}, height: {targetSize.Height}", extended: true);

        _portalCamera.targetTexture = _viewTexture;

        LinkedPortal?.SetScreenRenderTexture(_viewTexture);
    }

    private bool IsRendering()
    {
        return _renderingContainer.activeSelf;
    }

    private void SetRendering(bool value)
    {
        _renderingContainer.SetActive(value);

        if (MainEntrance == null)
            return;

        UpdateNightVision();
        UpdateDoor();
    }

    private void UpdateNightVision()
    {
        if (MainEntrance == null)
            return;

        if (MainEntrance.IsOutside)
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

        Transform linkedScreen = LinkedPortal._screen.transform;
        Transform thisScreen = _screen.transform;

        Matrix4x4 linkedFlipped = linkedScreen.localToWorldMatrix * Matrix4x4.Rotate(Quaternion.Euler(0f, 180f, 0f));
        Matrix4x4 relativeTransform = thisScreen.localToWorldMatrix * Matrix4x4.Inverse(linkedFlipped);

        _portalCamera.transform.SetPositionAndRotation(
            relativeTransform.MultiplyPoint(playerCamera.transform.position),
            relativeTransform.rotation * playerCamera.transform.rotation
        );

        _portalCamera.fieldOfView = playerCamera.fieldOfView;

        SetFarClipPlane();
        SetNearClipPlane();

        LevelHelper.SetSunAndSkyEnabledThisFrame(MainEntrance.IsOutside);

        _portalCamera.Render();

        LevelHelper.SetSunAndSkyEnabledThisFrame(!MainEntrance.IsOutside);
    }

    private void SetFarClipPlane()
    {
        if (MainEntrance == null)
            return;

        float farClipPlane;

        if (PortalSettings.UseViewDistance.Value)
        {
            farClipPlane = PortalSettings.ViewDistance.Value;
        }
        else
        {
            if (MainEntrance.IsOutside)
            {
                farClipPlane = ConfigManager.PortalGraphics_OutsideViewDistance.Value;
            }
            else
            {
                farClipPlane = ConfigManager.PortalGraphics_InsideViewDistance.Value;
            }
        }

        float distanceFromScreen = Vector3.Distance(_portalCamera.transform.position, _screen.transform.position);

        _portalCamera.farClipPlane = farClipPlane + distanceFromScreen;
    }

    private void SetNearClipPlane()
    {
        NearClipPlaneMode mode = ConfigManager.Debug_NearClipPlaneMode.Value;

        if (mode == NearClipPlaneMode.ObliqueProjection)
        {
            SetObliqueNearClipPlane();
            return;
        }

        SetDefaultNearClipPlane();
    }

    private void SetDefaultNearClipPlane()
    {
        Transform screenTransform = _screen.transform;

        Vector3 localCameraPos = screenTransform.InverseTransformPoint(_portalCamera.transform.position);

        MeshFilter meshFilter = _screen.GetComponent<MeshFilter>();
        Bounds localBounds = meshFilter?.sharedMesh?.bounds ?? new Bounds(Vector3.zero, Vector3.one);

        var closestLocalPoint = new Vector3(
            Mathf.Clamp(localCameraPos.x, localBounds.min.x, localBounds.max.x),
            Mathf.Clamp(localCameraPos.y, localBounds.min.y, localBounds.max.y),
            0f
        );

        Vector3 closestWorldPoint = screenTransform.TransformPoint(closestLocalPoint);

        // Project the vector from the portal camera to the closest point onto the
        // camera's forward axis — this gives the depth the near plane must be at
        // to not clip past the closest point at any angle
        Vector3 toClosest = closestWorldPoint - _portalCamera.transform.position;
        float projectedDistance = Vector3.Dot(toClosest, _portalCamera.transform.forward);

        _portalCamera.ResetProjectionMatrix();
        _portalCamera.nearClipPlane = Mathf.Max(0.01f, projectedDistance);
    }

    /*
     * This makes fog look really weird because of a Unity HDRP bug with CalculateObliqueMatrix
     */
    private void SetObliqueNearClipPlane()
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

        _portalCamera.nearClipPlane = 0.05f;
        _portalCamera.projectionMatrix = _portalCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    public static bool TryGetRenderingInstance(out DoorPortal doorPortal)
    {
        doorPortal = _instances.FirstOrDefault(x => x.IsRendering());
        return doorPortal != null;
    }
    #endregion

    private void UpdateDoor()
    {
        if (MainEntrance == null)
            return;

        if (IsRendering())
        {
            MainEntrance.EntranceObjects.ViewBlocker.SetActive(false);
        }
        else
        {
            MainEntrance.EntranceObjects.ViewBlocker.SetActive(!_isDrawing);
        }

        bool hideDoorObjects = ConfigManager.Debug_HideDoorObjects.Value;

        if (hideDoorObjects)
        {
            MainEntrance.EntranceObjects.SetObjectsEnabled(false);
        }
        else
        {
            MainEntrance.EntranceObjects.SetObjectsEnabled(!IsRendering());
        }
    }

    #region Config
    private void ApplyConfigSettings()
    {
        ApplyCameraConfigSettings();

        UpdateDoor();

        bool excludeFogBehindScreen = ConfigManager.Debug_ExcludeFogBehindScreen.Value;
        _fogExclusionZone?.SetActive(excludeFogBehindScreen);
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
