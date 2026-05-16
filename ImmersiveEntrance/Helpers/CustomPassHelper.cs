using com.github.zehsteam.ImmersiveEntrance.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace com.github.zehsteam.ImmersiveEntrance.Helpers;

internal static class CustomPassHelper
{
    public static GameObject OldVolume { get; private set; }
    public static GameObject NewVolume { get; private set; }
    public static CustomPass OldPass { get; private set; }
    public static CustomPass NewPass { get; private set; }

    public static void ReplaceVanillaCustomPass()
    {
        OldVolume = GameObject.Find("CustomPass (1)"); // Previously "CustomPass"
        Transform renderingParent = OldVolume.transform.parent;

        CustomPassVolume oldPassVolume = OldVolume.GetComponent<CustomPassVolume>();

        NewVolume = new GameObject($"{MyPluginInfo.PLUGIN_NAME} CustomPass");
        NewVolume.transform.SetParent(renderingParent);

        CustomPassVolume newPassVolume = NewVolume.AddComponent<CustomPassVolume>();
        newPassVolume.injectionPoint = CustomPassInjectionPoint.BeforeTransparent;

        NewPass = new PosterizationCustomPass();

        newPassVolume.customPasses.Add(NewPass);

        foreach (CustomPass customPass in oldPassVolume.customPasses)
        {
            if (customPass.name == "LethalSponge") // Previously "FS"
            {
                OldPass = customPass;
                OldPass.enabled = false;
                break;
            }
        }
    }
}
