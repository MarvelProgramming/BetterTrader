using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class Trader
    {
        public int CurrentCoins { get; private set; } = 1000;
        public bool RefreshInventoryAtInterval { get; private set; } = true;
        public int InventoryRefreshInterval { get; private set; } = 1;
        public int DaysSinceLastInventoryRefresh { get; private set; }
        public int GlobalItemPriceScalar { get; private set; } = 1;
        [YamlIgnore]
        public List<ITradableConfig> ItemConfigurations;
        private List<ICirculatedItem> ItemsInCirculation { get; set; } = new List<ICirculatedItem>();

        // Empty .ctor required for YamlDotNet to work with this type.
        public Trader() { }

        public Trader(int currentCoins, bool refreshInventoryAtInterval, int inventoryRefreshInterval, int daysSinceLastInventoryRefresh, int globalItemPriceScalar, List<ICirculatedItem> currentItems)
        {
            CurrentCoins = currentCoins;
            RefreshInventoryAtInterval = refreshInventoryAtInterval;
            InventoryRefreshInterval = inventoryRefreshInterval;
            DaysSinceLastInventoryRefresh = daysSinceLastInventoryRefresh;
            GlobalItemPriceScalar = globalItemPriceScalar;
            ItemsInCirculation = currentItems;
        }

        public List<ICirculatedItem> GetItemsInCirculation()
        {
            return new List<ICirculatedItem>(ItemsInCirculation);
        }

        public int GetNumberOfItemsInCirculation()
        {
            return ItemsInCirculation.Count;
        }

        public void UpdateCirculatedItems()
        {
            // TODO: populate "ItemsInCirculation" based on settings
            ItemsInCirculation.Clear();

            foreach(ITradableConfig item in ItemConfigurations)
            {
                var circulatedItem = new CirculatedItem(item.Name);
                ItemsInCirculation.Add(circulatedItem);
            }
        }
    }
}
