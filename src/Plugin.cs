using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    private static string _modDirectory = string.Empty;
    public static string ModDirectory => (_modDirectory == string.Empty) ? _modDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) : _modDirectory;

    public void OnEnable()
    {
        try
        {
            Logger = base.Logger;

            On.RainWorld.OnModsInit += OnModsInit;
            RemixAutoRestart.AddHooks(); // must be applied early
        }
        catch (Exception e) { Logger.LogError(e); }
    }
    
    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        MachineConnector.SetRegisteredOI("MenuFixes", Options.Instance);

        if (initialized) return;
        initialized = true;

        List<string> activeMods = ModManager.ActiveMods.ConvertAll(new Converter<ModManager.Mod, string>((mod) => { return mod.id; }));

        if (!activeMods.Contains("ScrollFix"))
            ScrollFix.AddHooks();
        if (!activeMods.Contains("OptimizedRemix") && !activeMods.Contains("FasterRemix"))
            OptimizedRemix.Init();
        if (!activeMods.Contains("magica.exactrequirements"))
            RemixExactRequirements.AddHooks();
        if (!activeMods.Contains("kevadroz.no_mod_update_confirm"))
            NoModUpdateConfirm.AddHooks();
    }
}
