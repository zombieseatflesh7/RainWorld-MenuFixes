using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MenuFixes.Mods;

// ModlistHotload by farge_goty
public static class ModlistHotload
{
    private static RainWorld rainWorldInstance;
    private static FileSystemWatcher modsWatcher;
    private static FileSystemWatcher workshopWatcher;

    private static List<string> modDirectories = new List<string>();
    private static List<string> lastMods = new List<string>();
    private static readonly object directoriesLock = new object();

    private static bool newMods = false;

    public static void AddHooks()
    {
        try
        {
            string gamePath = Path.GetDirectoryName(Application.dataPath);
            string modsPath = Path.Combine(gamePath, "RainWorld_Data", "StreamingAssets", "mods");
            string workshopPath = Path.Combine(gamePath, "..", "..", "workshop", "content", "312520");

            lock (directoriesLock)
            {
                modDirectories.Clear();
                lastMods.Clear();

                if (Directory.Exists(modsPath))
                {
                    modDirectories.AddRange(Directory.GetDirectories(modsPath));
                    SetupWatcher(ref modsWatcher, modsPath);
                }

                if (Directory.Exists(workshopPath))
                {
                    modDirectories.AddRange(Directory.GetDirectories(workshopPath));
                    SetupWatcher(ref workshopWatcher, workshopPath);
                }

                foreach (string dir in modDirectories)
                {
                    if (File.Exists(Path.Combine(dir, "modinfo.json")))
                    {
                        lastMods.Add(dir);
                    }
                }
            }

            On.Menu.MainMenu.ModListButtonPressed += MainMenuModListButtonPressed;
            Plugin.Logger.LogInfo("Loaded Modlist Hotload");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Modlist Hotload");
            Plugin.Logger.LogError(e);
        }
    }

    private static void CheckForNewMods()
    {
        bool modsChanged = false;
        List<string> currentMods = new List<string>();

        lock (directoriesLock)
        {
            foreach (string dir in modDirectories)
            {
                string modInfoPath = Path.Combine(dir, "modinfo.json");
                if (File.Exists(modInfoPath))
                {
                    currentMods.Add(dir);
                }
            }

            modsChanged = currentMods.Count != lastMods.Count ||
                          !currentMods.SequenceEqual(lastMods);

            if (modsChanged)
            {
                lastMods.Clear();
                lastMods.AddRange(currentMods);
            }
        }

        if (modsChanged)
        {
            newMods = true;
        }
    }

    private static void RefreshModsList()
    {
        try
        {
            if (rainWorldInstance == null) return;
            ModManager.RefreshModsLists(rainWorldInstance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error refreshing mods: {e}");
        }
    }

    public static void DisposeWatchers()
    {
        if (modsWatcher != null)
        {
            modsWatcher.Created -= OnNewDirectoryAdded;
            modsWatcher.Deleted -= OnNewDirectoryRemoved;
            modsWatcher.Dispose();
            modsWatcher = null;
        }

        if (workshopWatcher != null)
        {
            workshopWatcher.Created -= OnNewDirectoryAdded;
            workshopWatcher.Deleted -= OnNewDirectoryRemoved;
            workshopWatcher.Dispose();
            workshopWatcher = null;
        }
    }

    private static void SetupWatcher(ref FileSystemWatcher watcher, string path)
    {
        if (watcher != null) return;

        watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.DirectoryName,
            IncludeSubdirectories = false
        };

        watcher.Created += OnNewDirectoryAdded;
        watcher.Deleted += OnNewDirectoryRemoved;
        watcher.EnableRaisingEvents = true;
    }

    private static void MainMenuModListButtonPressed(On.Menu.MainMenu.orig_ModListButtonPressed orig, Menu.MainMenu self)
    {
        try
        {
            CheckForNewMods();
            if (newMods)
            {
                rainWorldInstance = self.manager.rainWorld;
                RefreshModsList();
                newMods = false;
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }

        orig(self);
    }

    private static void OnNewDirectoryAdded(object sender, FileSystemEventArgs e)
    {
        modDirectories.Add(e.FullPath);
    }

    private static void OnNewDirectoryRemoved(object sender, FileSystemEventArgs e)
    {
        modDirectories.Remove(e.FullPath);
    }
}