using HarmonyLib;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.patches
{
    [HarmonyPatch(typeof(ItemDrop))]
    internal class ItemDrop_Patches
    {
        [HarmonyPatch(nameof(ItemDrop.Awake)), HarmonyPostfix]
        public static void Awake(ItemDrop __instance)
        {
            BetterTraderClient.LoggerInstance.LogInfo(__instance.gameObject.name);
        }
    }
}
