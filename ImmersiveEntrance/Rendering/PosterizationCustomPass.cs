using com.github.zehsteam.ImmersiveEntrance.Managers;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.RendererUtils;

namespace com.github.zehsteam.ImmersiveEntrance.Rendering;

/**
 * This CustomPass is based off of the LethalSponge SpongeCustomPass.
 * https://github.com/CassCoffey/LethalSponge
 * Licensed under MIT License by Scoops
 */
public class PosterizationCustomPass : CustomPass
{
    public static Material PosterizationMaterial;
    public static Shader PosterizationShader;
    public static RTHandle PosterizationRT;

    // Exclusion Mask
    public static RTHandle ExclusionMaskRT;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        PosterizationShader = Assets.PosterizeShader;

        PosterizationRT = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32,
            useDynamicScale: true, name: "Posterization Buffer"
        );

        PosterizationMaterial = CoreUtils.CreateEngineMaterial(PosterizationShader);

        // Exclusion Mask
        ExclusionMaskRT = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.R8_UNorm, // Single channel, just 0 or 1
            useDynamicScale: true, name: "Exclusion Mask Buffer"
        );
    }

    protected override void Execute(CustomPassContext ctx)
    {
        CoreUtils.SetRenderTarget(ctx.cmd, ExclusionMaskRT, ctx.cameraDepthBuffer, ClearFlag.Color, Color.white);

        var rendererListDesc = new RendererListDesc([new ShaderTagId("PosterizeExclusion")], ctx.cullingResults, ctx.hdCamera.camera)
        {
            rendererConfiguration = PerObjectData.None,
            renderQueueRange = RenderQueueRange.all,
            sortingCriteria = SortingCriteria.None,
        };

        CoreUtils.DrawRendererList(ctx.renderContext, ctx.cmd, ctx.renderContext.CreateRendererList(rendererListDesc));

        ctx.propertyBlock.SetFloat("_OutlineThickness", 0.001f);
        ctx.propertyBlock.SetFloat("_DepthThreshold", 0.4f);
        ctx.propertyBlock.SetFloat("_DepthCurve", 0.4f);
        ctx.propertyBlock.SetFloat("_DepthStrength", 6f);
        ctx.propertyBlock.SetFloat("_ColorThreshold", 0.47f);
        ctx.propertyBlock.SetFloat("_ColorCurve", 2.94f);
        ctx.propertyBlock.SetFloat("_ColorStrength", 0.65f);
        ctx.propertyBlock.SetTexture("_ExclusionMaskBuffer", ExclusionMaskRT);

        bool useSimulatedDeviceDepth = ConfigManager.Debug_UseSimulatedDeviceDepth.Value;
        ctx.propertyBlock.SetInt("_UseSimulatedDeviceDepth", useSimulatedDeviceDepth ? 1 : 0);

        CoreUtils.SetRenderTarget(ctx.cmd, PosterizationRT, ClearFlag.All);
        CoreUtils.DrawFullScreen(ctx.cmd, PosterizationMaterial, ctx.propertyBlock, PosterizationMaterial.FindPass("ReadColor"));

        ctx.propertyBlock.SetTexture("_PosterizationBuffer", PosterizationRT);

        CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ClearFlag.None);
        CoreUtils.DrawFullScreen(ctx.cmd, PosterizationMaterial, ctx.propertyBlock, PosterizationMaterial.FindPass("WriteColor"));
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(PosterizationMaterial);
        PosterizationRT.Release();

        // Exclusion Mask
        ExclusionMaskRT.Release();
    }
}
