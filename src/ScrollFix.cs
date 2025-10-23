using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace MenuFixes;

// ScrollFix by Zombieseatflesh7
public static class ScrollFix
{
    public static void AddHooks()
    {
        try
        {
            IL.Menu.Menu.Update += Menu_UpdateIL;
            On.MainLoopProcess.RawUpdate += MainLoopProcess_RawUpdate;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Scroll Fix");
            Plugin.Logger.LogError(e.Message + "\n" + e.StackTrace);
        }
    }

    private static void MainLoopProcess_RawUpdate(On.MainLoopProcess.orig_RawUpdate orig, MainLoopProcess self, float dt)
    {
        // update scroll delta
        if (self.manager.currentMainLoop == self && self.manager.menuesMouseMode)
        {
            if (self is Menu.Menu)
                (self as Menu.Menu).floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
            else if (self is RainWorldGame && (self as RainWorldGame).pauseMenu != null)
                (self as RainWorldGame).pauseMenu.floatScrollWheel += Input.GetAxis("Mouse ScrollWheel");
        }
        orig(self, dt);
    }

    private static void Menu_UpdateIL(ILContext il)
    {
        ILCursor c = new(il);

        // remove scroll delta update, because it is handled in RawUpdate now
        c.GotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchLdarg(0),
            i => i.MatchLdfld("Menu.Menu", "floatScrollWheel")
            );
        c.RemoveRange(7);

        // number edit from 15f to 10f
        c.GotoNext(i => i.MatchLdcR4(15f));
        c.Remove();
        c.Emit(OpCodes.Ldc_R4, 10f);
    }
}
