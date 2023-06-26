using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{
    [HarmonyPatch(typeof(ZNet))]
    internal class ZNet_Patches
    {
        [HarmonyPatch(nameof(ZNet.Save)), HarmonyPostfix]
        private static void Save()
        {
            if (ZNet.instance != null && (ZNet.IsSinglePlayer || ZNet.instance.IsServer()))
            {
                EventManager.RaiseGameSave();
            }
        }
    }
}
