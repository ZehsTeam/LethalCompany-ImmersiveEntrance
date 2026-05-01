using com.github.zehsteam.PeekInside.Helpers;
using GameNetcodeStuff;
using UnityEngine;

namespace com.github.zehsteam.PeekInside.MonoBehaviours;

public class DoorPortal : MonoBehaviour
{
    #region Unity Editor
    [SerializeField]
    private MeshRenderer _screenMeshRenderer;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private GameObject _renderContainer;

    [SerializeField]
    private Material _outsideDefaultInputMaterial;

    [SerializeField]
    private Material _insideDefaultInputMaterial;

    [Space(10f)]
    [Header("Camera Output Templates")]
    [Space(5f)]

    [SerializeField]
    private RenderTexture _templateRenderTexture;

    [SerializeField]
    private Material _templateMaterial;
    #endregion

    private EntranceTeleport _entranceTeleport;
    private DoorPortal _linkedPortal;

    private RenderTexture _outputRenderTexture;
    private Material _outputMaterial;

    private Material _inputMaterial;

    public void SetEntranceTeleport(EntranceTeleport entranceTeleport)
    {
        _entranceTeleport = entranceTeleport;
    }

    private void LinkPortal(DoorPortal other)
    {
        if (_linkedPortal != null)
            return;

        if (this == other)
            return;

        _linkedPortal = other;
        _inputMaterial = other._outputMaterial;
        _screenMeshRenderer.material = _inputMaterial;

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked entranceId: {_entranceTeleport.entranceId} -> {other._entranceTeleport.entranceId}, isEntranceToBuilding: {_entranceTeleport.isEntranceToBuilding} -> {other._entranceTeleport.isEntranceToBuilding}", extended: true);
    }

    public static void LinkPortals(DoorPortal left, DoorPortal right)
    {
        if (left == null || right == null)
            return;

        if (left == right)
            return;

        if (left._linkedPortal != null || right._linkedPortal != null)
            return;

        left.LinkPortal(right);
        right.LinkPortal(left);

        Logger.LogInfo($"[{nameof(DoorPortal)}] Linked two portals!", extended: true);
    }

    private void Awake()
    {
        CreateCameraOutput();
    }

    private void CreateCameraOutput()
    {
        int randomId = Random.Range(1000000, 9999999);

        _outputRenderTexture = new RenderTexture(_templateRenderTexture)
        {
            name = $"{MyPluginInfo.PLUGIN_NAME} {randomId}"
        };

        Logger.LogInfo($"[{nameof(DoorPortal)}] RenderTexture created: {_outputRenderTexture.width}x{_outputRenderTexture.height}, isCreated: {_outputRenderTexture.IsCreated()}", extended: true);

        _outputMaterial = new Material(_templateMaterial)
        {
            name = $"{MyPluginInfo.PLUGIN_NAME} {randomId}",
            mainTexture = _outputRenderTexture
        };

        _camera.targetTexture = _outputRenderTexture;
    }

    private void Update()
    {
        if (_linkedPortal == null)
            return;

        UpdateScreen();
    }

    private void UpdateScreen()
    {
        if (IsLocalPlayerCameraNearby())
        {
            _screenMeshRenderer.material = _inputMaterial;

            _linkedPortal._renderContainer.SetActive(true);
        }
        else
        {
            if (_entranceTeleport.isEntranceToBuilding)
            {
                _screenMeshRenderer.material = _outsideDefaultInputMaterial;
            }
            else
            {
                _screenMeshRenderer.material = _insideDefaultInputMaterial;
            }
            
            _linkedPortal._renderContainer.SetActive(false);
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

    private void LateUpdate()
    {
        if (_linkedPortal == null)
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
        Transform linkedTransform = _linkedPortal.transform;

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
        Transform clipPlane = _linkedPortal.transform;

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
