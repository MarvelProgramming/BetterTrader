using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal class Trader
    {
        public int CurrentCoins { get; private set; } = 1000;
        public bool RefreshInventoryAtInterval { get; private set; } = true;
        public int InventoryRefreshInterval { get; private set; } = 1;
        public int DaysSinceLastInventoryRefresh { get; private set; }
        public int GlobalItemPriceScalar { get; private set; } = 1;
        public List<ITradable> CurrentItems { get; private set; } = new List<ITradable>();

        public Trader() { }

        public Trader(int currentCoins, bool refreshInventoryAtInterval, int inventoryRefreshInterval, int daysSinceLastInventoryRefresh, int globalItemPriceScalar, List<ITradable> currentItems)
        {
            CurrentCoins = currentCoins;
            RefreshInventoryAtInterval = refreshInventoryAtInterval;
            InventoryRefreshInterval = inventoryRefreshInterval;
            DaysSinceLastInventoryRefresh = daysSinceLastInventoryRefresh;
            GlobalItemPriceScalar = globalItemPriceScalar;
            CurrentItems = currentItems;
        }
    }
}
