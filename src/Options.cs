using Menu.Remix.MixedUI;
using UnityEngine;

namespace MenuFixes;

internal class Options : OptionInterface
{
    public static readonly Options Instance = new Options();

    public Configurable<bool> NMUC_onModUpdate;
    public Configurable<bool> NMUC_onModReload;
    public Configurable<bool> NMUC_onGameUpdate;


    public Options()
    {
        NMUC_onModUpdate = config.Bind("MenuFixes_NMUC_onModUpdate", true);
        NMUC_onModReload = config.Bind("MenuFixes_NMUC_onModReload", true);
        NMUC_onGameUpdate = config.Bind("MenuFixes_NMUC_onGameUpdate", false);
    }

    public override void Initialize()
    {
        // No Mod Update Confirm
        OpTab opTab = new(this, "NMUC");
        Tabs = new OpTab[1] { opTab };
        UIelement[] elements = new UIelement[6]
        {
                new OpLabel(40f, 550f, "Skip mod update confirm", bigText: true),
                new OpCheckBox(NMUC_onModUpdate, new Vector2(10f, 550f)),
                new OpLabel(40f, 450f, "Skip mod reload confirm", bigText: true),
                new OpCheckBox(NMUC_onModReload, new Vector2(10f, 450f)),
                new OpLabel(40f, 350f, "Skip game update confirm", bigText: true),
                new OpCheckBox(NMUC_onGameUpdate, new Vector2(10f, 350f))
        };
        opTab.AddItems(elements);

    }

}