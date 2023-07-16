using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Minimap))]
    internal class Minimap_Patches
    {
        [HarmonyPatch(nameof(Minimap.SetMapMode)), HarmonyPrefix]
        public static void SetMapMode(ref bool __runOriginal)
        {
            if (StoreGui.instance.m_trader != null && StoreGui.instance.m_trader.IsHaldor())
            {
                __runOriginal = false;
            }
        }
    }
}
