using IL;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MenuFixes;

internal class Options : OptionInterface
{
    public static readonly Options Instance = new Options();

    public static Configurable<bool> ORM_LoadThumbnails = Instance.config.Bind("OR_LoadThumbnails", true);
    public static Configurable<bool> ORM_ResizeLocalThumbnails = Instance.config.Bind("OR_ResizeLocalThumbnails", false);
    public static Configurable<bool> RAR_Enabled = Instance.config.Bind("RAR_Enabled", true);
    public static Configurable<bool> RAR_UseSteam = Instance.config.Bind("RAR_UseSteam", false);
    public static Configurable<bool> NMUC_onModUpdate = Instance.config.Bind("NMUC_onModUpdate", true);
    public static Configurable<bool> NMUC_onModReload = Instance.config.Bind("NMUC_onModReload", true);
    public static Configurable<float> NMUC_delay = Instance.config.Bind("NMUC_delay", 0.0f, new ConfigAcceptableRange<float>(0.0f, 5.0f));

    public static void EarlyLoadConfigs()
    {
        try
        {

            Instance.config.GetConfigPath();
            string path = Path.Combine(ConfigHolder.configDirPath, "MenuFixes" + ".txt");
            if (File.Exists(path))
            {
                var lines = File.ReadLines(path);
                foreach (string rawLine in lines)
                {
                    string line = rawLine.Trim();
                    if (line.StartsWith("#"))
                        continue;

                    string[] words = line.Split(['='], 2);
                    if (words.Length != 2)
                        continue;

                    string key = words[0].Trim();
                    string value = words[1].Trim();
                    try
                    {
                        switch (key)
                        {
                            case "OR_LoadThumbnails":
                                ORM_LoadThumbnails.Value = bool.Parse(value); break;
                            case "OR_ResizeLocalThumbnails":
                                ORM_ResizeLocalThumbnails.Value = bool.Parse(value); break;
                            case "RAR_Enabled":
                                RAR_Enabled.Value = bool.Parse(value); break;
                            case "RAR_UseSteam":
                                RAR_UseSteam.Value = bool.Parse(value); break;
                            case "NMUC_onModUpdate":
                                NMUC_onModUpdate.Value = bool.Parse(value); break;
                            case "NMUC_onModReload":
                                NMUC_onModReload.Value = bool.Parse(value); break;
                            case "NMUC_delay":
                                NMUC_delay.Value = float.Parse(value); break;
                        }
                    }
                    catch (Exception e) { Plugin.Logger.LogError(e); }
                }
                //foreach (ConfigurableBase cb in Instance.config.configurables.Values)
                //    Plugin.Logger.LogMessage($"{cb.key} = {cb.BoxedValue}");
            }
            else
                Plugin.Logger.LogError($"Unable to find config file in: {path}");
        }
        catch (Exception e) { Plugin.Logger.LogError(e); }
    }

    public override void Initialize()
    {
        // No Mod Update Confirm
        OpTab tab = new(this, "Options");
        Tabs = [tab];

        float margin = 20;
        float top = 480;
        float width = 300;
        float spacing = 10;
        float y = top - margin;
        float x = 300 - width / 2 + margin;
        UIelement[] elements = [
            new OpLabel(new(300, 500), default, "Many Menu Fixes", FLabelAlignment.Center, bigText: true),

            new OpCheckBox(ORM_LoadThumbnails, new(x, y -= 24))
            { description = "Disabling mod thumbnails will reduce ram usage and stuttering" },
            new OpLabel(x + 40, y + 2, "Mod thumbnails"),

            new OpCheckBox(ORM_ResizeLocalThumbnails, new(x, y -= 24 + spacing))
            { description = "Resizes local mod thumbnails during startup. This is reduces thumbnail load time, but causes problems if you develop mods" },
            new OpLabel(x + 40, y + 2, "Resize local mod thumbnails"),

            new OpCheckBox(RAR_Enabled, new(x, y -= 24 + 30))
            { description = "Automatically restart the game when it is required to reload mods" },
            new OpLabel(x + 40, y + 2, "Auto restart"),

            new OpCheckBox(RAR_UseSteam, new(x, y -= 24 + spacing))
            { description = "Might fix some issues, such as the Bepinex console log. Might not work on every machine." },
            new OpLabel(x + 40, y + 2, "Restart through Steam"),

            new OpCheckBox(NMUC_onModUpdate, new(x, y -= 24 + 30))
            { description = "Skips the confirmirmation dialog after mods update on the startup screen" },
            new OpLabel(new(x + 40, y + 2), default, "Skip mod update confirm", FLabelAlignment.Left),

            new OpCheckBox(NMUC_onModReload, new(x, y -= 24 + spacing))
            { description = "Skips the confirmmation dialog after applying changes in the remix menu" },
            new OpLabel(new(x + 40, y + 2), default, "Skip mod reload confirm", FLabelAlignment.Left),

            new OpUpdown(NMUC_delay, new(x, y -= 32 + spacing), 60)
            { description = "Additional delay before skipping the confirmation dialog" },
            new OpLabel(new(x + 80, y + 6), default, "Skip confirm delay", FLabelAlignment.Left),
        ];
        float bottom = y - margin;
        tab.AddItems(new OpRect(new(300 - width / 2, bottom), new(width, top - bottom)));
        tab.AddItems(elements);

    }

}