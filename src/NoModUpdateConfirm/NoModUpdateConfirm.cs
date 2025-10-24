using System;
using System.Collections.Generic;
using UnityEngine;

namespace MenuFixes;

// NoModUpdateConfirm by Kevadroz
public static class NoModUpdateConfirm
{
    private static Options options => Options.Instance;
    private static List<WeakReference<Menu.DialogBoxNotify>> clicked = new List<WeakReference<Menu.DialogBoxNotify>>();

    private static bool WasClicked(Menu.DialogBoxNotify dialog)
    {

        foreach (WeakReference<Menu.DialogBoxNotify> reference in clicked)
        {
            Menu.DialogBoxNotify dialog2;
            if (reference.TryGetTarget(out dialog2) && dialog.Equals(dialog2))
                return true;
        }

        return false;
    }

    private static void AddToClicked(Menu.DialogBoxNotify dialog)
    {
        clicked.Add(new WeakReference<Menu.DialogBoxNotify>(dialog));
    }

    private static void PurgeClicked()
    {
        foreach (WeakReference<Menu.DialogBoxNotify> reference in clicked)
        {
            if (!reference.TryGetTarget(out _))
            {
                clicked.Remove(reference);
            }
        }
    }

    public static void AddHooks()
    {
        On.Menu.DialogBoxNotify.ctor += OnNewDialogBox;
        On.Menu.DialogBoxNotify.Update += OnDialogBoxUpdate;
        Plugin.Logger.LogInfo("Loaded No Mod Update Confirm");
    }

    private static bool ShouldAutoConfirm(string signalText)
    {
        return (options.NMUC_onModUpdate.Value && signalText == "REAPPLY")
        || (options.NMUC_onModReload.Value && signalText == "RESTART")
        || (options.NMUC_onGameUpdate.Value && signalText == "VERSIONPROMPT");
    }

    private static void OnNewDialogBox(On.Menu.DialogBoxNotify.orig_ctor orig, Menu.DialogBoxNotify self, Menu.Menu menu, Menu.MenuObject owner, string text, string signalText, Vector2 pos, Vector2 size, bool forceWrapping)
    {
        orig(self, menu, owner, text, signalText, pos, size, forceWrapping);
        try
        {
            if (ShouldAutoConfirm(signalText))
            {
                self.RemoveSprites();
                PurgeClicked();
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    private static void OnDialogBoxUpdate(On.Menu.DialogBoxNotify.orig_Update orig, Menu.DialogBoxNotify self)
    {
        orig(self);
        try
        {
            if (ShouldAutoConfirm(self.continueButton.signalText) && !WasClicked(self))
            {
                self.continueButton.Clicked();
                AddToClicked(self);
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

}