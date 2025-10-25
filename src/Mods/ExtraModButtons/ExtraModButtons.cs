using Menu;
using Menu.Remix.MixedUI;
using Steamworks;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MenuFixes.Mods;

// Workshop button by farge_goty
// File explorer button by DarkNinja

public static class ExtraModButtons
{
    private static ModManager.Mod _previewMod;

    private static OpSimpleImageButton btnWorkshopView;
    private static OpSimpleImageButton btnFileExplorerView;

    public static bool showWorkshopButton = false;
    public static bool showFileExplorerButton = false;

    public static void Init()
    {
        if (showWorkshopButton || showFileExplorerButton)
            AddHooks();
    }

    private static void AddHooks()
    {
        try
        {
            On.Menu.Remix.InternalOI_Stats.Initialize += InternalOI_Stats_InitializeHook;
            On.Menu.Remix.InternalOI_Stats._PreviewMod += InternalOI_Stats__PreviewModHook;
            On.Menu.Remix.InternalOI_Stats.UnloadOI += InternalOI_Stats_UnloadOI;
            Plugin.Logger.LogInfo("Loaded Extra Mod Buttons");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Extra Mod Buttons");
            Plugin.Logger.LogError(e);
        }
    }

    private static void ShowModInWorkshop(UIfocusable trigger)
    {
        if (_previewMod != null)
        {
            SteamFriends.ActivateGameOverlayToWebPage($"https://steamcommunity.com/sharedfiles/filedetails/?id={_previewMod.workshopId}", EActivateGameOverlayToWebPageMode.k_EActivateGameOverlayToWebPageMode_Default);
        }
    }

    private static void ShowModInFileExplorer(UIfocusable trigger)
    {
        try
        {
            if (_previewMod != null)
            {
                OpenUrl(_previewMod.basePath);
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch (Exception ex)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Plugin.Logger.LogError(ex);
            }
        }
    }

    private static void InternalOI_Stats_InitializeHook(On.Menu.Remix.InternalOI_Stats.orig_Initialize orig, Menu.Remix.InternalOI_Stats self)
    {
        orig(self);

        try
        {
            if (showWorkshopButton)
            {
                Futile.atlasManager.LoadAtlas("assets/WorkshopButton_Icons");
                btnWorkshopView = new OpSimpleImageButton(new Vector2(560f, 440f), new Vector2(30f, 30f), "WorkshopButton_Icon")
                {
                    description = ModdingMenu.instance.Translate("View the mod on the Steam Workshop")
                };
                btnWorkshopView.OnClick += ShowModInWorkshop;
                self.Tabs[1].AddItems(btnWorkshopView);
            }
            
            if (showFileExplorerButton)
            {
                Futile.atlasManager.LoadAtlas("assets/ModInExplorerButton_Icons");
                btnFileExplorerView = new OpSimpleImageButton(new Vector2(520f, 440f), new Vector2(30f, 30f), "ModInExplorerButton_Icon")
                {
                    description = ModdingMenu.instance.Translate("View the mod in your file explorer")
                };
                btnFileExplorerView.OnClick += ShowModInFileExplorer;
                self.Tabs[1].AddItems(btnFileExplorerView);
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    private static void InternalOI_Stats__PreviewModHook(On.Menu.Remix.InternalOI_Stats.orig__PreviewMod orig, Menu.Remix.InternalOI_Stats self, Menu.Remix.MenuModList.ModButton button)
    {
        orig(self, button);

        try
        {
            if (btnWorkshopView != null)
            {
                if (self.previewMod.workshopMod)
                {
                    _previewMod = self.previewMod;
                    btnWorkshopView.Show();
                    btnWorkshopView.PosY = self.lblName.PosY;
                }
                else
                {
                    btnWorkshopView.Hide();
                }
            }

            if (btnFileExplorerView != null)
            {
                if (self.previewMod.basePath != null)
                {
                    _previewMod = self.previewMod;
                    btnFileExplorerView.Show();
                    btnFileExplorerView.PosY = self.lblName.PosY;
                }
                else
                {
                    btnFileExplorerView.Hide();
                }
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    private static void InternalOI_Stats_UnloadOI(On.Menu.Remix.InternalOI_Stats.orig_UnloadOI orig, Menu.Remix.InternalOI_Stats self)
    {
        orig(self);

        if (showWorkshopButton)
        {
            btnWorkshopView = null;
            Futile.atlasManager.UnloadAtlas("assets/WorkshopButton_Icons");
        }

        if (showWorkshopButton)
        {
            btnFileExplorerView = null;
            Futile.atlasManager.UnloadAtlas("assets/ModInExplorerButton_Icons");
        }
    }
}
