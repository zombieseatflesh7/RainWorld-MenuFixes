using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Security.Permissions;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace MenuFixes;

[BepInPlugin("zombieseatflesh7.MenuFixes", "Menu Fixes", "1.0.0")]
sealed class Plugin : BaseUnityPlugin
{
    public static new BepInEx.Logging.ManualLogSource Logger;

    public void OnEnable()
    {
        Logger = base.Logger;

    }
}
