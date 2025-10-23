using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MenuFixes;

// RemixAutoRestart by Gamer025
public static class RemixAutoRestart
{
    public static void AddHooks()
    {
        try
        {
            On.Menu.InitializationScreen.Singal += InitializationScreen_Signal;
            On.Menu.ModdingMenu.Singal += ModdingMenu_Singal;
        }
        catch (Exception e) 
        {
            Plugin.Logger.LogError("Failed to load RemixAutoRestart");
            UnityEngine.Debug.LogException(e); 
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
            psi.Arguments = String.Join(" ", new_args.ToArray());
            Process.Start(psi);
            UnityEngine.Application.Quit();
        }
        catch (Exception e) { UnityEngine.Debug.LogException(e); }
    }
}