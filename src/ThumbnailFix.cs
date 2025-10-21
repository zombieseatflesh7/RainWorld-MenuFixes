using Menu;
using Menu.Remix;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MenuFixes; 
public static class ThumbnailFix
{
    public static FShader thumbnailShader;

    public static void AddHooks()
    {
        try
        {
            thumbnailShader = FShader.SolidColored;

            On.Menu.Remix.ConfigContainer.QueueModThumbnails += (orig, self, buttons) => { }; // This loads all thumbnails immediately. Fuck off.

            IL.Menu.Remix.InternalOI_Stats.Initialize += InternalOI_Stats_Initialize_IL; // changed OpImage to use atlas element instead of texture
            IL.Menu.Remix.InternalOI_Stats._PreviewMod += PreviewMod_IL; // Fix to load and display unloaded thumbnails immediately

            // experimenting. probably gonna scrap this
            IL.Menu.Remix.MenuModList.ModButton.ctor += ModButton_Ctor_IL;
            On.Menu.Remix.MenuModList.ModButton._ProcessThumbnail += ModButton_ProcessThumbnail;
            On.Menu.Remix.MenuModList.ModButton._UpdateThumbnail += ModButton_UpdateThumbnail;
            On.Menu.Remix.MenuModList.ModButton.UnloadUI += ModButton_UnloadUI;

            On.Menu.Remix.ConfigContainer._LoadModThumbnail += LoadModThumbnail;

            /*On.FAtlasManager.AddAtlas += (orig, self, atlas) => // logging new atlas creation. reduce this as much as possible
            {
                orig(self, atlas);
                string stacktrace = string.Join("\n", Environment.StackTrace.Split('\n').Skip(3));
                Plugin.Logger.LogDebug($"{atlas.name}\n{Environment.StackTrace}");
            };*/
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("Failed to load Thumbnail Fix");
            Plugin.Logger.LogError(e.Message + "\n" + e.StackTrace);
        }
        
    }

    private static void ModButton_Ctor_IL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(
            i => i.MatchLdarg(0),
            i => i.MatchLdsfld<MenuModList.ModButton>("_thumbD")
            );
        int start = c.Index;
        c.GotoNext(i => i.MatchStfld<MenuModList.ModButton>("_thumbnail"));
        int end = c.Index + 1;

        c.Goto(start);
        c.RemoveRange(end - start);
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((MenuModList.ModButton b) =>
        {
            b._thumb = MenuModList.ModButton._thumbD;
            b._thumbnail = new FTexture(b._thumb, b.itf.mod.id)
            {
                anchorX = 0f,
                anchorY = 0f,
                x = 125f - (float)b._thumb.width * MenuModList.ModButton._thumbRatio / 2f - 6f,
                y = 9f,
                scaleX = MenuModList.ModButton._thumbRatio,
                scaleY = MenuModList.ModButton._thumbRatio
            };
        });
    }

    private static void ModButton_ProcessThumbnail(On.Menu.Remix.MenuModList.ModButton.orig__ProcessThumbnail orig, MenuModList.ModButton self)
    {
        if (!self._thumbLoaded)
            ConfigContainer.instance._LoadModThumbnail(self);

        self._thumbProcessed = true;
        if (!self._thumbBlank)
        {
            self._thumb = Futile.atlasManager.GetAtlasWithName(ConfigContainer._GetThumbnailName(self.itf.mod.id)).texture as Texture2D;
            self._UpdateThumbnail();
        }
    }

    private static void ModButton_UpdateThumbnail(On.Menu.Remix.MenuModList.ModButton.orig__UpdateThumbnail orig, MenuModList.ModButton self)
    {
        if (!self._thumbBlank)
        {
            if (self.selectEnabled)
            {
                self._thumbnail.SetTexture(self._thumb);
            }
            else
            {
                Texture2D thumbnail = self._thumb.Clone();
                MenuColorEffect.TextureGreyscale(ref thumbnail);
                self._thumbnail.SetTexture(thumbnail);
                Object.Destroy(thumbnail);
            }
        }
    }

    private static void ModButton_UnloadUI(On.Menu.Remix.MenuModList.ModButton.orig_UnloadUI orig, MenuModList.ModButton self)
    {
        self._thumbnail.Destroy();
    }

    private static void InternalOI_Stats_Initialize_IL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        // Texture2D image = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
        // imgThumbnail = new OpImage(default(Vector2), image);
        c.GotoNext( 
            i => i.MatchLdcI4(1),
            i => i.MatchLdcI4(1),
            i => i.MatchLdcI4(5),
            i => i.MatchLdcI4(0),
            i => i.MatchNewobj<Texture2D>(),
            i => i.MatchStloc(2)
            );
        c.RemoveRange(13);

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((InternalOI_Stats stats) =>
        {
            Plugin.Logger.LogWarning("Constructing InternalOI_Stats thumbnail OpImage");
            stats.imgThumbnail = new Menu.Remix.MixedUI.OpImage(default, ConfigContainer._GetThumbnailName(MenuModList.ModButton.RainWorldDummy.mod.id));
        });
    }

    private static void PreviewMod_IL(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        c.GotoNext(
            i => i.MatchLdloc(0),
            i => i.MatchLdfld<ModManager.Mod>("id"),
            i => i.MatchCall<ConfigContainer>("_GetThumbnailName")
            );
        int index = c.Index + 4; // IL_0048: ldsfld class FAtlasManager Futile::atlasManager
        // if (atlasWithName != null)
        c.GotoNext(i => i.Match(OpCodes.Brfalse_S)); // IL_0055: brfalse.s IL_00c0 // beginning of if block
        // else
        c.GotoLabel(c.Next.Operand as ILLabel); // IL_00c0: ldarg.1 // beginning of else block
        ILLabel destination = c.Prev.Operand as ILLabel; // label points to end of if / else block
        
        c.Goto(index);
        c.Emit(OpCodes.Ldarg_0); // this
        c.Emit(OpCodes.Ldarg_1); // MenuModList.ModButton button
        c.EmitDelegate((InternalOI_Stats stats, MenuModList.ModButton button) =>
        {
            Plugin.Logger.LogWarning($"Preview Mod: {button.itf.mod.id}");
            string thumbnailName = ConfigContainer._GetThumbnailName(button.itf.mod.id);
            if (!button._thumbLoaded && !Futile.atlasManager.DoesContainAtlas(thumbnailName))
                ConfigContainer.instance._LoadModThumbnail(button);

            if (button._thumbBlank) // button._thumbnail blank is probably an uneccessary check
            {
                stats.imgThumbnail.Hide(); 
                // TODO load default image
            }
            else
            {
                stats.imgThumbnail.ChangeElement(thumbnailName);
                stats.imgThumbnail.Show();
            }
        });
        c.Emit(OpCodes.Br, destination); // skip over the vanilla code for loading the thumbnail
    }

    private static int LoadModThumbnail(On.Menu.Remix.ConfigContainer.orig__LoadModThumbnail orig, ConfigContainer self, MenuModList.ModButton button)
    {
        Plugin.Logger.LogWarning($"Loading mod thumbnail: {button.itf.mod.id}");
        return orig(self,button);
    }
}
