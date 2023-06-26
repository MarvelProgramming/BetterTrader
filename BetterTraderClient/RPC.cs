using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    public static class RPC
    {
        private static List<ICirculatedItem> requestedPurchasableItems = new List<ICirculatedItem>();
        private static int lastRequestID = -1;
        private static bool finishedItemRequest = true;

        public static void RegisterRPCMethods()
        {
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderCoins), RPC_RequestTraderCoins);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailablePurchaseItems), RPC_RequestAvailablePurchaseItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailableSellItems), RPC_RequestAvailableSellItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestPurchaseItem), RPC_RequestPurchaseItem);
            RPCUtils.RegisterMethod(nameof(RPC_RequestSellItem), RPC_RequestSellItem);
        }

        [Client]
        public static void RPC_RequestTraderCoins(long sender, ZPackage pkg)
        {
            int traderCoins = pkg.ReadInt();
            EventManager.RaiseFetchedTraderCoins(traderCoins);
        }

        [Client]
        public static void RPC_RequestAvailablePurchaseItems(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                ZPackage itemPkg = pkg.ReadPackage();
                int requestID = itemPkg.ReadInt();
                int totalItemCount = itemPkg.ReadInt();
                int itemsInPackage = itemPkg.ReadInt();

                if (finishedItemRequest || lastRequestID != requestID)
                {
                    requestedPurchasableItems.Clear();
                    finishedItemRequest = false;
                    lastRequestID = requestID;
                }

                for (int i = 0; i < itemsInPackage; i++)
                {
                    ICirculatedItem item = new CirculatedItem();
                    item.Deserialize(ref itemPkg);
                    requestedPurchasableItems.Add(item);
                }

                if (requestedPurchasableItems.Count == totalItemCount)
                {
                    List<ICirculatedItem> availableItems = new List<ICirculatedItem>();

                    foreach(ICirculatedItem item in requestedPurchasableItems)
                    {
                        if (item.RequireDiscovery && !Player.m_localPlayer.m_knownMaterial.Contains(item.Drop.m_itemData.m_shared.m_name))
                        {
                            continue;
                        }

                        availableItems.Add(item);
                    }

                    finishedItemRequest = true;
                    EventManager.RaiseFetchedAvailablePurchaseItems(availableItems);
                }
                else if (requestedPurchasableItems.Count > totalItemCount)
                {
                    BetterTraderClient.LoggerInstance.LogError
                        (
                        "Received more items from request than expected! There may be an error " +
                        "in what the server said the total should be in the payload, or the collection " +
                        "of requested items isn't be cleared at the appropriate time."
                        );
                }
            }
        }

        [Client]
        public static void RPC_RequestAvailableSellItems(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                ZPackage itemsPkg = pkg.ReadPackage();
                int itemsCount = itemsPkg.ReadInt();
                List<ICirculatedItem> sellableItems = new List<ICirculatedItem>();
                
                for(int i = 0; i < itemsCount; i++)
                {
                    var item = new CirculatedItem();
                    item.Deserialize(ref itemsPkg);
                    sellableItems.Add(item);
                }

                EventManager.RaiseFetchedAvailableSellItems(sellableItems);
            }
        }

        [Client]
        public static void RPC_RequestPurchaseItem(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                bool canPurchase = pkg.ReadBool();

                if (canPurchase)
                {
                    string itemToPurchase = pkg.ReadString();
                    int quantity = pkg.ReadInt();
                    int quantityLeftToAdd = quantity;
                    int totalPurchasePrice = pkg.ReadInt();
                    ItemDrop purchaseItemDrop = ObjectDB.instance.m_itemByHash[itemToPurchase.GetStableHashCode()].GetComponent<ItemDrop>();
                    
                    while(quantityLeftToAdd > 0)
                    {
                        int quantityToAdd = Mathf.Min(quantityLeftToAdd, purchaseItemDrop.m_itemData.m_shared.m_maxStackSize);
                        quantityLeftToAdd -= quantityToAdd;
                        Player.m_localPlayer.m_inventory.AddItem(purchaseItemDrop.gameObject, quantityToAdd);
                    }

                    Player.m_localPlayer.m_inventory.RemoveItem(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name, totalPurchasePrice);
                    StoreGui.instance.m_buyEffects.Create(StoreGui.instance.transform.position, Quaternion.identity);
                    Player.m_localPlayer.ShowPickupMessage(purchaseItemDrop.m_itemData, quantity);
                    StoreGui.instance.m_trader.OnBought(null);
                    Gogan.LogEvent("Game", "BoughtItem", purchaseItemDrop.name, 0L);
                    EventManager.RaisePlayerCoinsChanged(Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name));
                }
            }
        }

        [Client]
        public static void RPC_RequestSellItem(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                bool canSell = pkg.ReadBool();

                if (canSell)
                {
                    ZPackage responsePackage = pkg.ReadPackage();
                    int quantity = responsePackage.ReadInt();
                    ICirculatedItem inventoryItem = new CirculatedItem();
                    inventoryItem.Deserialize(ref responsePackage);
                    int totalSellPrice = inventoryItem.CurrentSalesPrice * quantity;
                    ItemDrop.ItemData sellItemDrop = Player.m_localPlayer.m_inventory.GetItemAt(inventoryItem.GridPosition.x, inventoryItem.GridPosition.y);
                    string sellText = quantity > 1 ? sellItemDrop.m_shared.m_name : $"{quantity}x{sellItemDrop.m_shared.m_name}";
                    Player.m_localPlayer.UnequipItem(sellItemDrop);
                    Player.m_localPlayer.m_inventory.RemoveItem(sellItemDrop, quantity);
                    Player.m_localPlayer.m_inventory.AddItem(StoreGui.instance.m_coinPrefab.gameObject.name, totalSellPrice, StoreGui.instance.m_coinPrefab.m_itemData.m_quality, StoreGui.instance.m_coinPrefab.m_itemData.m_variant, 0L, "");
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_sold", sellText, totalSellPrice.ToString()), 0, sellItemDrop.m_shared.m_icons[0]);
                    StoreGui.instance.m_sellEffects.Create(StoreGui.instance.transform.position, Quaternion.identity);
                    StoreGui.instance.m_trader.OnSold();
                    Gogan.LogEvent("Game", "SoldItem", sellText, 0L);
                    EventManager.RaisePlayerCoinsChanged(Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name));
                }
            }
        }
    }
}
