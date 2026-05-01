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
    private Material _darkMaterial;

    [Space(10f)]
    [Header("Camera Output Templates")]
    [Space(5f)]

    [SerializeField]
    private RenderTexture _templateRenderTexture;

    [SerializeField]
    private Material _templateMaterial;
    #endregion

    private DoorPortal _linkedPortal;

    private RenderTexture _outputRenderTexture;
    private Material _outputMaterial;

    private Material _inputMaterial;

    private void LinkPortal(DoorPortal other)
    {
        if (_linkedPortal != null)
            return;

        if (this == other)
            return;

        _linkedPortal = other;
        _inputMaterial = other._outputMaterial;
        _screenMeshRenderer.material = _inputMaterial;
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
    }

    private void Awake()
    {
        CreateCameraOutput();
    }

    private void CreateCameraOutput()
    {
        _outputRenderTexture = new RenderTexture(_templateRenderTexture);

        _outputMaterial = new Material(_templateMaterial)
        {
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
        if (IsLocalPlayerNearby())
        {
            _screenMeshRenderer.material = _inputMaterial;
            _linkedPortal._camera.enabled = true;
        }
        else
        {
            _screenMeshRenderer.material = _darkMaterial;
            _linkedPortal._camera.enabled = false;
        }
    }

    private bool IsLocalPlayerNearby()
    {
        return PlayerUtils.IsLocalPlayerNearby(transform.position, radius: 5f);
    }

    private void LateUpdate()
    {
        if (_linkedPortal == null)
            return;

        UpdateCamera();
    }

    private void UpdateCamera()
    {
        if (!_camera.enabled)
            return;

        Camera playerCamera = GetLocalPlayerCamera();

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
