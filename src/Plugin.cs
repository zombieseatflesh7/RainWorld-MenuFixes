using BepInEx;
using HarmonyLib;
using Menu.Remix;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuFixes;

[BepInPlugin("zombieseatflesh7.MenuFixes", "Menu Fixes", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static new BepInEx.Logging.ManualLogSource Logger;

    public void OnEnable()
    {
        Logger = base.Logger;
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        // TODO: compatibility check with existing mods that do the same thing

        ScrollFix.AddHooks();
        ThumbnailFix.AddHooks(); // This hook crashes the game if its applied OnEnable

        orig(self);
    }
}
