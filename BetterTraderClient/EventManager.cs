using System;
using System.Collections.Generic;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal static class EventManager
    {
        public static Action<int, List<ICirculatedItem>> OnFetchedTraderInfo;
        public static Action<ItemPanel> OnMousePointerEnterItemPanel;
        public static Action OnMousePointerExitItemPanel;
        public static Action<ItemPanel> OnMouseClickedItemPanel;

        public static void RaiseFetchedTraderInventory(int coins, List<ICirculatedItem> items)
        {
            OnFetchedTraderInfo?.Invoke(coins, items);
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
