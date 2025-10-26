using Menu;
using System;
using System.Collections.Generic;

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
        PurgeDeadDialogs();

        foreach (KeyValuePair<WeakReference<DialogBoxNotify>, float> entry in trackedDialogs)
        {
            WeakReference<DialogBoxNotify> reference = entry.Key;
            if (reference.TryGetTarget(out DialogBoxNotify dialog2))
            {
                if (dialog.Equals(dialog2))
                {

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
        }
        WeakReference<DialogBoxNotify> newReference = new(dialog);
        if (delay > 0.0f)
        {
            trackedDialogs.Add(newReference, delay);
            return false;
        }
        else
        {
            trackedDialogs.Add(newReference, float.NegativeInfinity);
            return true;
        }
    }

    private static void MarkDialogClicked(DialogBoxNotify dialog)
    {
        PurgeDeadDialogs();

        foreach (KeyValuePair<WeakReference<DialogBoxNotify>, float> entry in trackedDialogs)
        {
            WeakReference<DialogBoxNotify> reference = entry.Key;
            if (reference.TryGetTarget(out DialogBoxNotify dialog2) && dialog.Equals(dialog2))
            {
                trackedDialogs[reference] = float.NegativeInfinity;
                return;
            }
        }

        trackedDialogs.Add(new WeakReference<DialogBoxNotify>(dialog), float.NegativeInfinity);
    }

    private static void PurgeDeadDialogs()
    {
        List<WeakReference<DialogBoxNotify>> referencesToRemove = [];

        foreach (WeakReference<DialogBoxNotify> reference in trackedDialogs.Keys)
            if (!reference.TryGetTarget(out _))
                referencesToRemove.Add(reference);

        foreach (WeakReference<DialogBoxNotify> reference in referencesToRemove)
            trackedDialogs.Remove(reference);
    }

    public static void AddHooks()
    {
        On.Menu.DialogBoxNotify.Update += OnDialogBoxUpdate;
        On.ModManager.CheckInitIssues += ModManager_CheckInitIssues;
        On.Menu.InitializationScreen.Singal += OnInitSingal;
        On.Menu.ModdingMenu.Singal += OnModMenuSingal;


        Plugin.Logger.LogInfo("Loaded No Mod Update Confirm");
    }

    private static void OnInitSingal(On.Menu.InitializationScreen.orig_Singal orig, InitializationScreen self, MenuObject sender, string message)
    {
        try
        {
            if (message == "RESTART" || message == "REAPPLY")
                foreach (MenuObject menuObject in self.pages[0].subObjects)
                    if (menuObject is DialogBoxNotify dialog && dialog.continueButton == sender)
                    {
                        MarkDialogClicked(dialog);
                        break;
                    }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }

        orig(self, sender, message);
    }

    private static void OnModMenuSingal(On.Menu.ModdingMenu.orig_Singal orig, ModdingMenu self, MenuObject sender, string message)
    {
        try
        {
            if (message == "RESTART" || message == "REAPPLY")
                foreach (MenuObject menuObject in self.pages[0].subObjects)
                    if (menuObject is DialogBoxNotify dialog && dialog.continueButton == sender)
                    {
                        Plugin.Logger.LogMessage("button clicked");
                        MarkDialogClicked(dialog);
                        break;
                    }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }

        orig(self, sender, message);
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
        orig(self);
        try
        {
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