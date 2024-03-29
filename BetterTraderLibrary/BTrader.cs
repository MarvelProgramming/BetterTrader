﻿using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class BTrader
    {
        public bool HasCoins { get; private set; } = true;
        public int BaseCoins { get; private set; } = 1000;
        public bool HasLimitedItems { get; private set; }
        public int UpperItemLimit { get; private set; } = 15;
        public int LowerItemLimit { get; private set; } = 5;
        public int PerItemStockMax { get; private set; } = 100;
        public bool HasRandomItems { get; private set; }
        public bool CanRepairItems { get; private set; } = true;
        public int PerItemRepairCost { get; private set; } = 10;
        public bool DepreciatingSaleValues { get; private set; } = true;
        public float SalesValueDepreciationScalar { get; private set; } = 0.8f;
        public int InventoryRefreshInterval { get; private set; } = 1;
        public int GlobalItemPriceScalar { get; private set; } = 1;
        [YamlIgnore]
        public int Coins
        {
            get => RealtimeData.CurrentCoins;
            private set
            {
                if (HasCoins)
                {
                    RealtimeData.CurrentCoins = value;
                }
            }
        }
        [YamlIgnore]
        public List<CirculatedItem> ItemsInCirculation => new List<CirculatedItem>(RealtimeData.ItemsInCirculation);
        [YamlIgnore]
        public int NumberOfItemsInCirculation => RealtimeData.ItemsInCirculation.Count;
        [YamlIgnore]
        public List<ITradableConfig> ItemConfigurations { get; set; }
        [YamlIgnore]
        public readonly Dictionary<int, Tuple<ICirculatedItem, ITradableConfig>> hashItemAssociations = new Dictionary<int, Tuple<ICirculatedItem, ITradableConfig>>();
        [YamlIgnore]
        public readonly List<Tuple<ICirculatedItem, ITradableConfig>> purchasableItemsList = new List<Tuple<ICirculatedItem, ITradableConfig>>();
        [YamlIgnore]
        public readonly List<Tuple<ICirculatedItem, ITradableConfig>> activelyPurchasableItemsList = new List<Tuple<ICirculatedItem, ITradableConfig>>();
        private TraderRealtimeData RealtimeData { get; set; }

        public class TraderRealtimeData
        {
            public int CurrentCoins { get; set; }
            public int DaysSinceLastInventoryRefresh { get; set; }
            public List<CirculatedItem> ItemsInCirculation { get; set; } = new List<CirculatedItem>();

            // Empty .ctor required for YamlDotNet to work with this type.
            public TraderRealtimeData() { }

            public TraderRealtimeData(int currentCoins, int daysSinceLastInventoryRefresh, List<CirculatedItem> itemsInCirculation)
            {
                CurrentCoins = currentCoins;
                DaysSinceLastInventoryRefresh = daysSinceLastInventoryRefresh;
                ItemsInCirculation = itemsInCirculation ?? new List<CirculatedItem>();
            }
        }

        // Empty .ctor required for YamlDotNet to work with this type.
        public BTrader() 
        {
            RealtimeData = new TraderRealtimeData();
        }

        public BTrader(bool hasCoins, int baseCoins, bool hasLimitedItems, int upperItemLimit, int lowerItemLimit, int perItemStockMax, bool hasRandomItems, bool canRepairItems, int perItemRepairCost, bool depreciatingSaleValues, float salesValueDepreciationScalar, int inventoryRefreshInterval, int globalItemPriceScalar, TraderRealtimeData realtimeData)
        {
            HasCoins = hasCoins;
            BaseCoins = baseCoins;
            HasLimitedItems = hasLimitedItems;
            UpperItemLimit = upperItemLimit;
            LowerItemLimit = lowerItemLimit;
            PerItemStockMax = perItemStockMax;
            HasRandomItems = hasRandomItems;
            CanRepairItems = canRepairItems;
            PerItemRepairCost = perItemRepairCost;
            DepreciatingSaleValues = depreciatingSaleValues;
            SalesValueDepreciationScalar = salesValueDepreciationScalar;
            InventoryRefreshInterval = inventoryRefreshInterval;
            GlobalItemPriceScalar = globalItemPriceScalar;
            RealtimeData = realtimeData;
        }

        public List<ICirculatedItem> GetItemsInCirculation()
        {
            return new List<ICirculatedItem>(RealtimeData.ItemsInCirculation);
        }

        public void GetInfo(ref ZPackage pkg)
        {
            pkg.Write(HasCoins);
            pkg.Write(Coins);
            pkg.Write(CanRepairItems);
            pkg.Write(PerItemRepairCost);
        }

        public void UpdateCirculatedItems(bool skipRefreshIntervalCheck = false)
        {
            List<Tuple<ICirculatedItem, ITradableConfig>> itemsToCirculate = new List<Tuple<ICirculatedItem, ITradableConfig>>(purchasableItemsList);

            if (!skipRefreshIntervalCheck && RealtimeData.DaysSinceLastInventoryRefresh < InventoryRefreshInterval)
            {
                return;
            }

            RealtimeData.CurrentCoins = BaseCoins;
            RealtimeData.DaysSinceLastInventoryRefresh = 0;

            foreach (Tuple<ICirculatedItem, ITradableConfig> circulatedItem in activelyPurchasableItemsList)
            {
                circulatedItem.Item1.IsActivelyPurchasable = false;
            }

            activelyPurchasableItemsList.Clear();

            if (HasRandomItems)
            {
                itemsToCirculate.Clear();

                for (int i = 0; i < Math.Min(UnityEngine.Random.Range(LowerItemLimit, UpperItemLimit), purchasableItemsList.Count); i++)
                {
                    Tuple<ICirculatedItem, ITradableConfig> item = purchasableItemsList[UnityEngine.Random.Range(0, purchasableItemsList.Count - 1)];
                    itemsToCirculate.Add(item);
                }
            }

            if (HasLimitedItems && itemsToCirculate.Count > UpperItemLimit)
            {
                itemsToCirculate.RemoveRange(0, itemsToCirculate.Count - UpperItemLimit);
            }

            foreach (Tuple<ICirculatedItem, ITradableConfig> item in itemsToCirculate)
            {
                bool isDiscounted = item.Item2.CanBeOnDiscount && Mathf.Lerp(0, UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)) > 0.8f;
                int purchasePrice = item.Item2.BasePurchasePrice;
                int salesPrice = item.Item2.BaseSalesPrice;

                if (isDiscounted)
                {
                    purchasePrice *= item.Item2.DiscountScalar;
                    salesPrice *= item.Item2.DiscountScalar;
                }

                purchasePrice *= GlobalItemPriceScalar;
                ICirculatedItem circulatedItem = item.Item1;
                circulatedItem.IsOnDiscount = isDiscounted;
                circulatedItem.CurrentPurchasePrice = purchasePrice;
                circulatedItem.CurrentSalesPrice = salesPrice;
                circulatedItem.CurrentStock = Mathf.Min(PerItemStockMax, item.Item2.BaseTraderStorage);
                circulatedItem.IsActivelyPurchasable = true;
                activelyPurchasableItemsList.Add(Tuple.Create(circulatedItem, item.Item2));
            }
        }

        public List<ICirculatedItem> GetItemsClientCanPurchase()
        {
            List<ICirculatedItem> purchasableItems = activelyPurchasableItemsList.Select(item => item.Item1).ToList();

            return purchasableItems;
        }

        public void GetItemsClientCanSell(List<ICirculatedItem> playerInventoryItems, ref ZPackage pkg)
        {
            List<ICirculatedItem> sellableItems = new List<ICirculatedItem>();

            foreach(ICirculatedItem inventoryItem in playerInventoryItems)
            {
                if (hashItemAssociations.TryGetValue(inventoryItem.Name.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
                {
                    if (!item.Item2.Sellable)
                    {
                        continue;
                    }

                    inventoryItem.CurrentSalesPrice = item.Item1.CurrentSalesPrice;
                    sellableItems.Add(inventoryItem);
                }
            }

            pkg.Write(sellableItems.Count);

            foreach (var item in sellableItems.Select(circulatedItem => circulatedItem as CirculatedItem))
            {
                item?.Serialize(ref pkg);
            }
        }

        public bool CanSell(string itemName, int quantity)
        {
            if (hashItemAssociations.TryGetValue(itemName.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
            {
                if (item.Item1.IsActivelyPurchasable && item.Item1.CurrentStock >= quantity)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanBeSold(int quantity, ICirculatedItem inventoryItem)
        {
            if (hashItemAssociations.TryGetValue(inventoryItem.Name.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
            {
                if (item.Item2.Sellable && (!HasCoins || Coins >= item.Item1.CurrentSalesPrice * quantity))
                {
                    return true;
                }
            }

            return false;
        }

        public void PurchaseItem(string itemName, int quantity)
        {
            if (hashItemAssociations.TryGetValue(itemName.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
            {
                Coins += item.Item1.CurrentPurchasePrice * quantity;
                item.Item1.CurrentStock -= quantity;

                if (item.Item1.CurrentStock == 0)
                {
                    item.Item1.IsActivelyPurchasable = false;
                    activelyPurchasableItemsList.RemoveAt(activelyPurchasableItemsList.FindIndex(purchasableItem => purchasableItem.Item1.Name == item.Item1.Name));
                }
            }
        }

        public void SellItem(string itemName, int quantity)
        {
            if (hashItemAssociations.TryGetValue(itemName.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
            {
                Coins -= item.Item1.CurrentSalesPrice * quantity;

                item.Item1.CurrentStock = Mathf.Min(item.Item1.CurrentSalesPrice + quantity, PerItemStockMax);

                if (DepreciatingSaleValues)
                {
                    item.Item1.CurrentSalesPrice = Mathf.Max((int)(item.Item1.CurrentSalesPrice * Mathf.Pow(SalesValueDepreciationScalar, quantity)), 0);
                }

                if (activelyPurchasableItemsList.All(purchasableItem => purchasableItem.Item1.Name != item.Item1.Name))
                {
                    item.Item1.IsActivelyPurchasable = true;
                    activelyPurchasableItemsList.Add(item);
                }
            }
        }

        public void RepairItems(int quantity)
        {
            Coins += PerItemRepairCost * quantity;
        }

        public void IncreaseDaysSinceLastInventoryRefresh()
        {
            RealtimeData.DaysSinceLastInventoryRefresh++;
        }

        public void UpdateAllItemAssociations()
        {
            hashItemAssociations.Clear();
            activelyPurchasableItemsList.Clear();
            purchasableItemsList.Clear();
            List<CirculatedItem> itemsToRemoveFromCirculation = new List<CirculatedItem>();

            foreach (ITradableConfig itemConfig in ItemConfigurations)
            {
                CirculatedItem circulatedItem = new CirculatedItem(itemConfig.Name, itemConfig.RequireDiscovery, false, false, itemConfig.BasePurchasePrice, itemConfig.BaseSalesPrice, Mathf.Min(itemConfig.BaseTraderStorage, PerItemStockMax));
                int itemNameHash = itemConfig.Name.GetStableHashCode();
                var itemRepresentation = Tuple.Create((ICirculatedItem)circulatedItem, itemConfig);

                if (itemConfig.Purchasable || itemConfig.Sellable)
                {
                    if (RealtimeData.ItemsInCirculation.All(item => item.Name != itemConfig.Name))
                    {
                        RealtimeData.ItemsInCirculation.Add(circulatedItem);
                    
                        if (itemConfig.Purchasable)
                        {
                            purchasableItemsList.Add(itemRepresentation);
                        }
                    }
                }
                else
                {
                    CirculatedItem itemInCirculation = RealtimeData.ItemsInCirculation.Find(itemInCirculation => itemInCirculation.Name == itemConfig.Name);

                    if (itemInCirculation != null)
                    {
                        itemsToRemoveFromCirculation.Add(itemInCirculation);
                    }
                }

                hashItemAssociations.Add(itemNameHash, itemRepresentation);
            }

            foreach (CirculatedItem item in itemsToRemoveFromCirculation)
            {
                RealtimeData.ItemsInCirculation.Remove(item);
            }

            if (HasLimitedItems && RealtimeData.ItemsInCirculation.Count > UpperItemLimit)
            {
                RealtimeData.ItemsInCirculation.RemoveRange(0, RealtimeData.ItemsInCirculation.Count - UpperItemLimit);
            }

            // Override cached circulated items with realtime values.
            foreach (ICirculatedItem circulatedItem in RealtimeData.ItemsInCirculation)
            {
                int itemNameHash = circulatedItem.Name.GetHashCode();

                if (hashItemAssociations.TryGetValue(itemNameHash, out Tuple<ICirculatedItem, ITradableConfig> item))
                {
                    Tuple<ICirculatedItem, ITradableConfig> itemRepresentation = Tuple.Create(circulatedItem, item.Item2);
                    circulatedItem.RequireDiscovery = item.Item2.RequireDiscovery;
                    circulatedItem.CurrentStock = Mathf.Min(circulatedItem.CurrentStock, PerItemStockMax);
                    hashItemAssociations[itemNameHash] = itemRepresentation;

                    if (item.Item2.Purchasable)
                    {
                        purchasableItemsList.Add(itemRepresentation);
                    }

                    if (circulatedItem.IsActivelyPurchasable)
                    {
                        activelyPurchasableItemsList.Add(itemRepresentation);
                    }
                }
            }
        }

        public ICirculatedItem GetItem(string name)
        {
            if (hashItemAssociations.TryGetValue(name.GetStableHashCode(), out Tuple<ICirculatedItem, ITradableConfig> item))
            {
                return item.Item1;
            }

            return null;
        }
    }
}
