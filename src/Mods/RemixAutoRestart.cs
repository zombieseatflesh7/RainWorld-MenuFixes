using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MenuFixes.Mods;

// RemixAutoRestart by Gamer025
public static class RemixAutoRestart
{
    private static bool enabled => Options.RAR_Enabled.Value;
    private static bool useSteam => Options.RAR_UseSteam.Value;
    private static bool hooked = false;
    private static bool restarting = false;

    public static void Init()
    {
        Options.Instance.OnConfigChanged += () =>
        {
            if (enabled && !hooked) AddHooks();
            else if (!enabled && hooked) RemoveHooks();
        };

        if (enabled)
            AddHooks();
    }

    public static void AddHooks()
    {
        hooked = true;
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

    private static void RemoveHooks()
    {
        try
        {
            hooked = false;
            On.Menu.InitializationScreen.Singal -= InitializationScreen_Signal;
            On.Menu.ModdingMenu.Singal -= ModdingMenu_Singal;
            Plugin.Logger.LogInfo("Unloaded Remix Auto Restarter");
        }
        catch (InvalidOperationException e) { }
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
            if (useSteam && RWCustom.Custom.rainWorld.processManager.mySteamManager != null) // steam version
            {
                string steamUrl = $"steam://rungameid/{312520}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = steamUrl,
                    UseShellExecute = true
                });
            }
            else // non - steam version
            {
                var process = Process.GetCurrentProcess();
                string fullPath = $"\"{process.MainModule.FileName}\"";

                var s_SavedEnv = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
                List<string> itemsToRemove = new List<string>();
                foreach (DictionaryEntry ent in s_SavedEnv)
                {
                    if (ent.Key.ToString().StartsWith("DOORSTOP"))
                    {
                        itemsToRemove.Add(ent.Key.ToString());
                    }
                }

                foreach (var item in itemsToRemove)
                    s_SavedEnv.Remove(item);

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.EnvironmentVariables.Clear();
                foreach (DictionaryEntry ent in s_SavedEnv)
                {
                    psi.EnvironmentVariables.Add((string)ent.Key, (string)ent.Value);
                }
                psi.UseShellExecute = false;
                psi.FileName = fullPath;

                //Command line args
                List<string> new_args = new List<string>();
                string[] current_args = Environment.GetCommandLineArgs();
                for (int i = 0; i < current_args.Length; i++)
                {
                    //Skip the first elements because that is the process file itself
                    if (i == 0)
                        continue;

                    //Something (Doorstop?) is adding a logFile arg to the process in the format "-logFile C:\path\to\Rain World\output.log"
                    //We need to skip that arg and the following one (the logfile path itself) otherwise the process args just keep growing with more and more -logFile args
                    if (current_args[i] == "-logFile")
                    {
                        i++;
                        continue;
                    }

                    new_args.Add(current_args[i]);
                }
                psi.Arguments = string.Join(" ", new_args.ToArray());
                Process.Start(psi);
            }

            UnityEngine.Application.Quit();
            restarting = true;
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }
}