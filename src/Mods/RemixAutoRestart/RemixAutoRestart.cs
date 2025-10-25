using System;
using System.Diagnostics;

namespace MenuFixes.Mods;

// RemixAutoRestart by Gamer025
public static class RemixAutoRestart
{
    private static bool restarting = false;

    public static void AddHooks()
    {
        try
        {
            On.Menu.InitializationScreen.Singal += InitializationScreen_Signal;
            On.Menu.ModdingMenu.Singal += ModdingMenu_Singal;
            Plugin.Logger.LogInfo("Loaded Remix Auto Restarter");
        }
        catch (Exception e) 
        {
            Plugin.Logger.LogError("Failed to load Remix Auto Restarter");
            Plugin.Logger.LogError(e); 
        }
    }

    private static void InitializationScreen_Signal(On.Menu.InitializationScreen.orig_Singal orig, Menu.InitializationScreen self, Menu.MenuObject sender, string message)
    {
        if (message == "RESTART")
        {
            Restart();
        }
        orig(self, sender, message);
    }

    private static void ModdingMenu_Singal(On.Menu.ModdingMenu.orig_Singal orig, Menu.ModdingMenu self, Menu.MenuObject sender, string message)
    {
        if (message == "RESTART")
        {
            Restart();
        }
        orig(self, sender, message);
    }

    public static void Restart()
    {
        if (restarting)
            return;

        try
        {
            string steamUrl = $"steam://rungameid/{312520}";
            Process.Start(new ProcessStartInfo
            {
                FileName = steamUrl,
                UseShellExecute = true
            });
            UnityEngine.Application.Quit();
            restarting = true;
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }
}