using BepInEx;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuFixes;

[BepInPlugin("zombieseatflesh7.MenuFixes", "Menu Fixes", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static new BepInEx.Logging.ManualLogSource Logger;
    private static bool initialized = false;
    private static string _modDirectory = string.Empty;
    public static string ModDirectory => (_modDirectory == string.Empty) ? _modDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) : _modDirectory; 

    public void OnEnable()
    {
        Logger = base.Logger;

        On.RainWorld.OnModsInit += OnModsInit;
    }

    private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        if (initialized) return;
        initialized = true;

        // TODO: compatibility check with existing mods that do the same thing

        ScrollFix.AddHooks();
        OptimizedRemix.Init();
    }
}
