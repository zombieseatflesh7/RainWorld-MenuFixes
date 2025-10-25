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
    
    public static Configurable<bool> NMUC_onModUpdate = Instance.config.Bind("NMUC_onModUpdate", true);
    public static Configurable<bool> NMUC_onModReload = Instance.config.Bind("NMUC_onModReload", true);
    public static Configurable<float> NMUC_delay = Instance.config.Bind("NMUC_delay", 0.0f, new ConfigAcceptableRange<float>(0.0f, 5.0f));

    public static void EarlyLoadConfigs(On.Menu.InitializationScreen.orig_ctor orig, InitializationScreen self, ProcessManager manager)
    {
        orig(self, manager);

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
        OpTab NMUC_Tab = new(this, "NMUC");
        Tabs = [NMUC_Tab];
        UIelement[] elements = [
            new OpLabel(new Vector2(150f, 520f), new Vector2(300f, 30f), "No Mod Update Confirm", FLabelAlignment.Center, bigText: true),
            new OpLabel(60f, 460f, "Skip mod update confirm"),
            new OpCheckBox(NMUC_onModUpdate, new Vector2(10f, 460f)) {
                description = "Skips the confirm dialog after updating the mods on the first load screen"
            },
            new OpLabel(60f, 430f, "Skip mod reload confirm"),
            new OpCheckBox(NMUC_onModReload, new Vector2(10f, 430f)) {
                description = "Skips the confirm dialog after applying changes in the remix menu"
            },
            new OpLabel(120f, 400f, "Delay"),
            new OpUpdown(NMUC_delay, new Vector2(10f, 395f), 100) {
                description = "Turn this up if the game closes before changes can be written"
            }
        ];
        NMUC_Tab.AddItems(elements);

    }

}