using BepInEx;
using MenuFixes.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuFixes;

[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "zombieseatflesh7.MenuFixes";
    public const string NAME = "Menu Fixes";
    public const string VERSION = "1.0.0";

    public static new BepInEx.Logging.ManualLogSource Logger;
    private static bool initialized = false;
    
    public void OnEnable()
    {
        Logger = base.Logger;

        On.Menu.InitializationScreen.ctor += PreInitialization;
        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void PreInitialization(On.Menu.InitializationScreen.orig_ctor orig, Menu.InitializationScreen self, ProcessManager manager)
    {
        try
        {
            Options.EarlyLoadConfigs();
            if (!Compat.NMUC())
                NoModUpdateConfirm.AddHooks();
            if (!Compat.RAR())
                Mods.RemixAutoRestart.Init();
        }
        catch (Exception e) { Logger.LogError(e); }

        orig(self, manager);
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        MachineConnector.SetRegisteredOI("MenuFixes", Options.Instance);

        if (initialized) return;
        initialized = true;

        List<string> activeMods = ModManager.ActiveMods.ConvertAll(mod => mod.id );

        if (!activeMods.Contains("OptimizedRemix") && !activeMods.Contains("FasterRemix"))
            OptimizedRemix.Init();
        if (!activeMods.Contains("ScrollFix"))
            ScrollFix.AddHooks();
        if (!activeMods.Contains("magica.exactrequirements"))
            RemixExactRequirements.AddHooks();
        if (!activeMods.Contains("fargegoty.workshopbutton"))
            ExtraModButtons.showWorkshopButton = true;
        if (!activeMods.Contains("darkninja.ModInExplorerButton"))
            ExtraModButtons.showFileExplorerButton = true;
        ExtraModButtons.Init();
        if (!activeMods.Contains("fargegoty.ModlistHotload"))
            ModlistHotload.AddHooks();
    }

    public void OnDisable()
    {
        ModlistHotload.DisposeWatchers();
    }
}
