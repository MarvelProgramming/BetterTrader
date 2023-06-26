using System;
using System.Collections.Generic;
using Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal static class EventManager
    {
        public static Action<string, string, Action, Action> OnNotification;
        public static Action OnPlayerInventoryChanged;
        public static Action<int> OnPlayerCoinsChanged;
        public static Action<int> OnFetchedTraderCoins;
        public static Action<List<ICirculatedItem>> OnFetchedAvailablePurchaseItems;
        public static Action<List<ICirculatedItem>> OnFetchedAvailableSellItems;
        public static Action<ItemPanel> OnMousePointerEnterItemPanel;
        public static Action OnMousePointerExitItemPanel;
        public static Action<ItemPanel> OnMouseClickedItemPanel;

        public static void RaiseNotification(string title, string description, Action accept, Action deny)
        {
            OnNotification?.Invoke(title, description, accept, deny);
        }

        public static void RaisePlayerInventoryChanged()
        {
            OnPlayerInventoryChanged?.Invoke();
        }

        public static void RaisePlayerCoinsChanged(int coins)
        {
            OnPlayerCoinsChanged?.Invoke(coins);
        }

        public static void RaiseFetchedTraderCoins(int coins)
        {
            OnFetchedTraderCoins?.Invoke(coins);
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
