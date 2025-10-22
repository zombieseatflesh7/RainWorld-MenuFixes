using BepInEx;
using HarmonyLib;
using Menu.Remix;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuFixes;

[BepInPlugin("zombieseatflesh7.MenuFixes", "Menu Fixes", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static new BepInEx.Logging.ManualLogSource Logger;
    private static bool initialized = false;

    public void OnEnable()
    {
        Logger = base.Logger;

        On.RainWorld.OnModsInit += OnModsInit;
    }

    // most of this code breaks if its run OnEnable for some reason lmao
    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (!initialized)
        {
            initialized = true;
            try
            {
                AssetBundle assetBundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("menufixesbundle"));
                self.Shaders.Add("MenuFixes.Greyscale", FShader.CreateShader("MenuFixes.Greyscale", assetBundle.LoadAsset<Shader>("Assets/Shaders/Greyscale.shader")));

                // TODO: compatibility check with existing mods that do the same thing

                ScrollFix.AddHooks();
                OptimizedRemix.AddHooks();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
