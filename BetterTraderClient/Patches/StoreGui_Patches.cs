using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(StoreGui))]
    internal class StoreGui_Patches
    {
        [HarmonyPatch(nameof(StoreGui.Awake)), HarmonyPostfix]
        public static void Awake(StoreGui __instance)
        {
            if (__instance.gameObject.name == "Store_Screen" && !StoreGUIUtils.GetBtUIExists(__instance, BetterTraderClient.UI_ASSET.name))
            {
                var btUI = Object.Instantiate(BetterTraderClient.UI_ASSET, __instance.transform);
                btUI.name = BetterTraderClient.UI_ASSET.name;
            }
        }

        [HarmonyPatch(nameof(StoreGui.Show)), HarmonyPostfix]
        public static void Show(StoreGui __instance, Trader trader)
        {
            if (trader.IsHaldor())
            {
                SetOriginalGUIVisibility(__instance, false);
                StoreGUIUtils.GetBtUIObject(__instance, BetterTraderClient.UI_ASSET.name)?.SetActive(true);
                EventManager.RaisePlayerCoinsChanged(Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name));
            }
        }

        [HarmonyPatch(nameof(StoreGui.Hide)), HarmonyPrefix]
        public static void Hide(StoreGui __instance, ref bool __runOriginal)
        {
            if (__instance.m_trader != null && __instance.m_trader.IsHaldor())
            {
                if (ZInput.GetButtonDown("Use")) 
                {
                    __runOriginal = false;
                    return;
                }

                SetOriginalGUIVisibility(__instance, true);
                StoreGUIUtils.GetBtUIObject(__instance, BetterTraderClient.UI_ASSET.name).SetActive(false);
            }
        }

        private static void SetOriginalGUIVisibility(StoreGui __instance, bool visibility)
        {
            // Effectively hides the original trade menu from view without confusing StoreGui into thinking it's been closed.
            foreach (Transform child in __instance.m_rootPanel.transform)
            {
                child.gameObject.SetActive(visibility);
            }
        }
    }
}
