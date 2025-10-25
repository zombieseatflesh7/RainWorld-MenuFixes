using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MenuFixes.Mods;

// RemixAutoRestart by Gamer025
public static class RemixAutoRestart
{
    private static bool restarting = false;

    public static void AddHooks()
    {
        try
        {
            IL.ModManager.RefreshModsLists += ModManager_RefreshModLists_IL;
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

    private static void ModManager_RefreshModLists_IL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext( // File.WriteAllText(Path.Combine(Custom.RootFolderDirectory(), "enabledModsVersion.txt"), "v1.11.3");
            i => i.MatchCall(AccessTools.Method(typeof(RWCustom.Custom), "RootFolderDirectory")),
            i => i.MatchLdstr("enabledModsVersion.txt"),
            i => i.MatchCall(AccessTools.Method(typeof(Path), "Combine", new Type[] { typeof(string), typeof(string)} ))
            );

        ILLabel destination = c.DefineLabel();
        c.EmitDelegate(() => // the same operation, but looped until it succeeds.
        {
            bool success = false;
            while(!success)
            {
                try
                {
                    Plugin.Logger.LogDebug("Writing to enabledModsVersion.txt.");
                    string version = typeof(RainWorld).GetField("GAME_VERSION_STRING").GetValue(null) as string;
                    File.WriteAllText(Path.Combine(RWCustom.Custom.RootFolderDirectory(), "enabledModsVersion.txt"), version);
                    success = true;
                }
                catch
                {
                    Plugin.Logger.LogWarning("Error writing to enabledModsVersion.txt. Trying again.");
                    Thread.Sleep(50);
                }
            }
        });
        c.Emit(OpCodes.Br_S, destination);
        c.Goto(c.Index + 5);
        c.MarkLabel(destination);
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
            UnityEngine.Application.Quit();
            restarting = true;
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }
}