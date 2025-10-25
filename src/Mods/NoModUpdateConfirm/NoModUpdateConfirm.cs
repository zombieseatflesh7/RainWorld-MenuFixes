using BepInEx.Logging;
using Menu;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MenuFixes.Mods;

// NoModUpdateConfirm by Kevadroz
public static class NoModUpdateConfirm
{
    private static bool onModUpdate => Options.NMUC_onModUpdate.Value;
    private static bool onModReload => Options.NMUC_onModReload.Value;
    private static float delay => Options.NMUC_delay.Value;
    private static readonly Dictionary<WeakReference<DialogBoxNotify>, float> trackedDialogs = [];
    private static bool initIssue = false;

    private static bool ShouldClickNow(DialogBoxNotify dialog)
    {
        List<WeakReference<DialogBoxNotify>> referencesToRemove = [];

        foreach (KeyValuePair<WeakReference<DialogBoxNotify>, float> entry in trackedDialogs)
        {
            WeakReference<DialogBoxNotify> reference = entry.Key;
            if (reference.TryGetTarget(out DialogBoxNotify dialog2))
            {
                if (dialog.Equals(dialog2))
                {
                    foreach (WeakReference<DialogBoxNotify> reference2 in referencesToRemove)
                        trackedDialogs.Remove(reference2);

                    float dialogTime = entry.Value;
                    if (float.IsNegativeInfinity(dialogTime))
                        return false;

                    float timeUntilClick = dialogTime - 1f / 40f;

                    if (timeUntilClick <= 0.0f)
                    {
                        trackedDialogs[reference] = float.NegativeInfinity;
                        return true;
                    }
                    else
                    {
                        trackedDialogs[reference] = timeUntilClick;
                        return false;
                    }
                }
            }
            else
                referencesToRemove.Add(reference);
        }

        foreach (WeakReference<DialogBoxNotify> reference in referencesToRemove)
            trackedDialogs.Remove(reference);

        if (delay > 0.0f)
        {
            trackedDialogs.Add(new WeakReference<DialogBoxNotify>(dialog), delay);
            return false;
        }
        else
        {
            trackedDialogs.Add(new WeakReference<DialogBoxNotify>(dialog), float.NegativeInfinity);
            return true;
        }
    }

    public static void AddHooks()
    {
        On.Menu.DialogBoxNotify.Update += OnDialogBoxUpdate;
        On.ModManager.CheckInitIssues += ModManager_CheckInitIssues;
        Plugin.Logger.LogInfo("Loaded No Mod Update Confirm");
    }

    private static bool ShouldAutoConfirm(DialogBoxNotify self)
    {
        return !self.continueButton.buttonBehav.greyedOut &&
                (self.menu is InitializationScreen || self.menu is ModdingMenu) &&
                 (onModUpdate && self.continueButton.signalText == "REAPPLY"
                || onModReload && self.continueButton.signalText == "RESTART");
    }

    private static void OnDialogBoxUpdate(On.Menu.DialogBoxNotify.orig_Update orig, DialogBoxNotify self)
    {
        try
        {
            orig(self);
            if (ShouldAutoConfirm(self) && ShouldClickNow(self) && !initIssue)
                self.continueButton.Clicked();
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    private static bool ModManager_CheckInitIssues(On.ModManager.orig_CheckInitIssues orig, Action<string> onIssue)
    {
        initIssue = orig(onIssue);
        return initIssue;
    }
}