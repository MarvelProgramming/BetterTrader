using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Player))]
    internal class Player_Patches
    {
        [HarmonyPatch(nameof(Player.OnInventoryChanged)), HarmonyPostfix]
        private static void OnInventoryChanged(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
            {
                EventManager.RaisePlayerInventoryChanged();
            }
        }
    }
}
