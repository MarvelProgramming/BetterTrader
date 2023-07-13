using Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal static class EventManager
    {
        public static Action<ItemPanel> OnInspectItemPanel;
        public static Action<string, string, Action, Action> OnNotification;
        public static Action OnPlayerRepairedItems;
        public static Action OnPlayerInventoryChanged;
        public static Action<int> OnPlayerCoinsChanged;
        public static Action<bool, int, bool, int> OnFetchedTraderInfo;
        public static Action<List<ICirculatedItem>> OnFetchedAvailablePurchaseItems;
        public static Action<List<ICirculatedItem>> OnFetchedAvailableSellItems;
        public static Action<ItemPanel> OnMousePointerEnterItemPanel;
        public static Action OnMousePointerExitItemPanel;
        public static Action<ItemPanel> OnMouseClickedItemPanel;

        public static void RaiseInspectItemPanel(ItemPanel itemPanel)
        {
            OnInspectItemPanel?.Invoke(itemPanel);
        }

        public static void RaiseNotification(string title, string description, Action accept, Action deny)
        {
            OnNotification?.Invoke(title, description, accept, deny);
        }

        public static void RaisePlayerRepairedItems()
        {
            OnPlayerRepairedItems();
        }

        public static void RaisePlayerInventoryChanged()
        {
            OnPlayerInventoryChanged?.Invoke();
        }

        public static void RaiseFetchedTraderInfo(bool hasCoins, int coins, bool canRepairItems, int perItemRepairCost)
        {
            OnFetchedTraderInfo?.Invoke(hasCoins, coins, canRepairItems, perItemRepairCost);
        }

        public static void RaisePlayerCoinsChanged(int coins)
        {
            OnPlayerCoinsChanged?.Invoke(coins);
        }

        public static void RaiseFetchedAvailablePurchaseItems(List<ICirculatedItem> items)
        {
            OnFetchedAvailablePurchaseItems?.Invoke(items);
        }

        public static void RaiseFetchedAvailableSellItems(List<ICirculatedItem> items)
        {
            OnFetchedAvailableSellItems?.Invoke(items);
        }

        public static void RaiseMousePointerEnterItemPanel(ItemPanel itemPanel)
        {
            OnMousePointerEnterItemPanel?.Invoke(itemPanel);
        }

        public static void RaiseMousePointerExitItemPanel()
        {
            OnMousePointerExitItemPanel?.Invoke();
        }

        public static void RaiseMouseClickedItemPanel(ItemPanel itemPanel)
        {
            OnMouseClickedItemPanel?.Invoke(itemPanel);
        }
    }
}
