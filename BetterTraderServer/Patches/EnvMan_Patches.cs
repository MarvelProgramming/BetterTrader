using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{
    [HarmonyPatch(typeof(EnvMan))]
    internal class EnvMan_Patches
    {
        [HarmonyPatch(nameof(EnvMan.OnMorning)), HarmonyPostfix]
        private static void OnMorning()
        {
            if (ZNet.instance != null && (ZNet.IsSinglePlayer || ZNet.instance.IsServer()))
            {
                EventManager.RaiseNewDay();
            }
        }
    }
}
