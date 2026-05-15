using com.github.zehsteam.ImmersiveEntrance.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.Rendering;

public class PortalClipCustomPass : CustomPass
{
    public static IReadOnlyList<PortalClipCustomPass> Instances => _instances;
    private static readonly List<PortalClipCustomPass> _instances = [];

    private static readonly int _portalPlaneNormalId = Shader.PropertyToID("_PortalPlaneNormal");
    private static readonly int _portalPlanePositionId = Shader.PropertyToID("_PortalPlanePosition");

    private Material _clipMaterial;

    private Vector3 _planeNormal;
    private Vector3 _planePosition;

    public static PortalClipCustomPass GetInstanceForCamera(Camera portalCamera)
    {
        if (portalCamera == null)
            return null;

        return _instances.FirstOrDefault(x => x.GetTargetCamera() == portalCamera);
    }

    public static bool TryGetInstanceForCamera(Camera portalCamera, out PortalClipCustomPass instance)
    {
        instance = GetInstanceForCamera(portalCamera);
        return instance != null;
    }

    public static void SetPortalPlane(Camera portalCamera, Vector3 normal, Vector3 position)
    {
        if (!TryGetInstanceForCamera(portalCamera, out PortalClipCustomPass instance))
            return;

        instance._planeNormal = normal;
        instance._planePosition = position;
    }

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        if (!_instances.Contains(this))
        {
            _instances.Add(this);
        }

        _clipMaterial = CoreUtils.CreateEngineMaterial(Assets.PortalClipShader);
    }

    protected override void Execute(CustomPassContext ctx)
    {
        _clipMaterial.SetVector(_portalPlaneNormalId, _planeNormal);
        _clipMaterial.SetVector(_portalPlanePositionId, _planePosition);

        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraDepthBuffer, ClearFlag.None);
        CoreUtils.DrawFullScreen(ctx.cmd, _clipMaterial, null, _clipMaterial.FindPass("PortalClip"));
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(_clipMaterial);

        _instances.Remove(this);
    }
}
