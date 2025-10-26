using Menu;
using Menu.Remix.MixedUI;
using System;
using System.CodeDom;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace MenuFixes;

internal class Options : OptionInterface
{
    public static readonly Options Instance = new Options();

    public static Configurable<bool> RemixAutoRestart_Enabled = Instance.config.Bind("RemixAutoRestart_Enabled", true);
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
                            case "NMUC_onModUpdate":
                                NMUC_onModUpdate.Value = bool.Parse(value); break;
                            case "NMUC_onModReload":
                                NMUC_onModReload.Value = bool.Parse(value); break;
                            case "NMUC_delay":
                                NMUC_delay.Value = float.Parse(value); break;
                            case "RemixAutoRestart_Enabled":
                                RemixAutoRestart_Enabled.Value = bool.Parse(value); break;
                        }
                    }
                    catch (Exception e) { Plugin.Logger.LogError(e); }
                }
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

        float y = 440f;
        UIelement[] elements = [
            new OpLabel(50f, y, "Remix Auto Restarter"),
            new OpCheckBox(RemixAutoRestart_Enabled, new Vector2(10f, y)) {
                description = "Automatically restart the game after applying mods"
            },
            new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), "Many Menu Fixes", FLabelAlignment.Center, bigText: true),
            new OpLabel(new(50f, y -= 60), default, "Skip mod update confirm", FLabelAlignment.Left),
            new OpCheckBox(NMUC_onModUpdate, new Vector2(10f, y)) {
                description = "Skips the confirm dialog after updating the mods on the first load screen"
            },
            new OpLabel(new(50f, y -= 40f), default, "Skip mod reload confirm", FLabelAlignment.Left),
            new OpCheckBox(NMUC_onModReload, new Vector2(10f, y)) {
                description = "Skips the confirm dialog after applying changes in the remix menu"
            },
            new OpLabel(new(80f, y -= 40f), default, "Skip confirm delay", FLabelAlignment.Left),
            new OpUpdown(NMUC_delay, new Vector2(10f, y), 60) {
                description = "Additional delay before skipping the confirm dialog"
            }
        ];
        tab.AddItems(elements);

    }

}