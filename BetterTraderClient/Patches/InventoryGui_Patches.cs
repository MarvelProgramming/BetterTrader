using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(InventoryGui))]
    internal class InventoryGui_Patches
    {
        [HarmonyPatch(nameof(InventoryGui.Show)), HarmonyPrefix]
        public static bool Show()
        {
            if (StoreGui.instance.m_trader != null && StoreGui.instance.m_trader.IsHaldor())
            {
                return false;
            }

            return true;
        }
    }
}
