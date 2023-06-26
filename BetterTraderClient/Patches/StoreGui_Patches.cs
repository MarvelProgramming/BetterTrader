using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderClient.Utils;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(StoreGui))]
    internal class StoreGui_Patches
    {
        [HarmonyPatch(nameof(StoreGui.Awake)), HarmonyPostfix]
        public static void Awake(StoreGui __instance)
        {
            if (!StoreGui_Utils.GetBtUIExists(__instance))
            {
                var btUI = Object.Instantiate(BetterTraderClient.UI_ASSET, __instance.transform);
                btUI.name = BetterTraderClient.UI_ASSET.name;
                
                // Effectively hides the original trade menu from view without confusing StoreGui into thinking it's been closed.
                foreach(Transform child in __instance.m_rootPanel.transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(nameof(StoreGui.Show)), HarmonyPostfix]
        public static void Show(StoreGui __instance, Trader trader)
        {
            if (trader.IsHaldor())
            {
                StoreGui_Utils.GetBtUIObject(__instance)?.SetActive(true);
                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestTraderCoins));
                EventManager.RaisePlayerCoinsChanged(Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name));
            }
        }

        [HarmonyPatch(nameof(StoreGui.Hide)), HarmonyPrefix]
        public static void Hide(StoreGui __instance)
        {
            if (__instance.m_trader != null && __instance.m_trader.IsHaldor())
            {
                StoreGui_Utils.GetBtUIObject(__instance).SetActive(false);
            }
        }
    }
}
