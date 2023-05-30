using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderClient.utils;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.patches
{
    [HarmonyPatch(typeof(StoreGui))]
    internal class StoreGui_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StoreGui.Awake))]
        public static void Awake(StoreGui __instance)
        {
            if (!StoreGui_Utils.GetBtUIExists(__instance))
            {
                var btUI = Object.Instantiate(BetterTraderClient.UI_ASSET, __instance.transform);
                btUI.name = BetterTraderClient.UI_ASSET.name;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StoreGui.Show))]
        public static void Show(StoreGui __instance, Trader trader)
        {
            if (trader?.m_name == "$npc_haldor")
            {
                StoreGui_Utils.GetBtUIObject(__instance)?.SetActive(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StoreGui.Hide))]
        public static void Hide(StoreGui __instance)
        {
            if (__instance.m_trader?.m_name == "$npc_haldor")
            {
                StoreGui_Utils.GetBtUIObject(__instance).SetActive(false);
            }
        }
    }
}
