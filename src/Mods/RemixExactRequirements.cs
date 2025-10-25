using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MenuFixes.Mods;

// Remix Exact Requirements by MagicaJaphet
public static class RemixExactRequirements
{
    private static HashSet<string> installedMods;

    public static void AddHooks()
    {
        try
        {
            installedMods = ModManager.ActiveMods.ConvertAll(new Converter<ModManager.Mod, string>((mod) => { return mod.id; })).ToHashSet();
            On.Menu.Remix.InternalOI_Stats.FailedRequirementsString += InternalOI_Stats_FailedRequirementsString;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Remix Exact Rquirements");
            Plugin.Logger.LogError(e);
        }
    }

    private static string InternalOI_Stats_FailedRequirementsString(On.Menu.Remix.InternalOI_Stats.orig_FailedRequirementsString orig, ModManager.Mod mod)
    {
        string result = orig(mod);
        try
        {
            HashSet<string> hashSet = new HashSet<string>();
            for (int i = 0; i < mod.requirements.Length; i++)
            {
                if (!installedMods.Contains(mod.requirements[i]))
                {
                    string text = OptionInterface.Translate(mod.requirements[i] + "-name");
                    if (text == mod.requirements[i] + "-name")
                    {
                        text = i >= mod.requirementsNames.Length || string.IsNullOrEmpty(mod.requirementsNames[i]) ? mod.requirements[i] : mod.requirementsNames[i];
                    }
                    hashSet.Add(text);
                }
            }
            if (hashSet.Count != 0)
            {
                return string.Join(", ", hashSet);
            }
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
        return result;
    }

}
