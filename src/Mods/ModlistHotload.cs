using IL.JollyCoop.JollyMenu;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MenuFixes.Mods;

// ModlistHotload by farge_goty
public static class ModlistHotload
{
    private static RainWorld rainWorld => RWCustom.Custom.rainWorld;

    private static FileSystemWatcher modsWatcher;
    private static FileSystemWatcher workshopWatcher;

    private static List<string> modDirectories = new List<string>();
    private static List<string> lastMods = new List<string>();
    private static readonly object directoriesLock = new object();

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
            On.Menu.ModdingMenu.Update += Update;

            On.Menu.Remix.MenuModList.GetModButton += MenuModList_GetModButton;

            Plugin.Logger.LogInfo("Loaded Modlist Hotload");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Modlist Hotload");
            Plugin.Logger.LogError(e);
        }
    }

    private static bool CheckForNewMods()
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

        return modsChanged;
    }

    private static void RefreshMenuModList()
    {
        if (RWCustom.Custom.rainWorld.processManager.currentMainLoop is not Menu.ModdingMenu menu)
            return;

        string[] selectedMods = ConfigContainer.menuTab.modList._currentSelections.Clone() as string[];

        // reload whole mod menu
        menu.pages[0].RemoveSubObject(menu.cfgContainer);
        menu.cfgContainer._ShutdownConfigContainer();
        menu.cfgContainer = new ConfigContainer(menu, menu.pages[0]);
        menu.pages[0].subObjects.Add(menu.cfgContainer);

        // reapply mod selections
        MenuModList modlist = ConfigContainer.menuTab.modList;

        foreach (MenuModList.ModButton btn in modlist.modButtons)
            if (btn.selectEnabled)
                modlist._ToggleMod_SubDisable(btn);

        for (int i = 0; i < selectedMods.Length; i++)
        {
            MenuModList.ModButton btn = modlist.GetModButton(selectedMods[i]);
            int order = i;
            if (btn != null)
                modlist._ToggleMod_SubEnable(btn, ref order);
        }
        modlist.RefreshAllButtons();
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

    private static void OnNewDirectoryAdded(object sender, FileSystemEventArgs e)
    {
        modDirectories = new List<string>();
    }

    private static void OnNewDirectoryRemoved(object sender, FileSystemEventArgs e)
    {
        modDirectories.Remove(e.FullPath);
    }

    private static float refreshTimer = 0;

    private static void MainMenuModListButtonPressed(On.Menu.MainMenu.orig_ModListButtonPressed orig, Menu.MainMenu self)
    {
        try
        {
            refreshTimer = 40 * 5;
            if (CheckForNewMods())
                ModManager.RefreshModsLists(rainWorld);
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }

        orig(self);
    }

    private static void Update(On.Menu.ModdingMenu.orig_Update orig, Menu.ModdingMenu self)
    {
        try
        { 
            if (refreshTimer > 0)
            {
                refreshTimer--;
                if (refreshTimer == 0)
                {
                    refreshTimer = self.framesPerSecond;
                    if (CheckForNewMods())
                    {
                        ModManager.RefreshModsLists(rainWorld);
                        RefreshMenuModList();
                    }
                }
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }

        orig(self);
    }

    private static MenuModList.ModButton MenuModList_GetModButton(On.Menu.Remix.MenuModList.orig_GetModButton orig, MenuModList self, string modID)
    {
        try // fix crash
        {
            return self.modButtons[ConfigContainer.FindItfIndex(modID)];
        }
        catch { return null; }
    }
}