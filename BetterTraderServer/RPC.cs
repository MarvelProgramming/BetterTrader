using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using UnityEngine;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal static class RPC
    {
        private static ConcurrentDictionary<long, Thread> inventoryRequests = new ConcurrentDictionary<long, Thread>();

        public static void RegisterRPCMethods()
        {
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderCoins), RPC_RequestTraderCoins);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailablePurchaseItems), RPC_RequestAvailablePurchaseItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailableSellItems), RPC_RequestAvailableSellItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestPurchaseItem), RPC_RequestPurchaseItem);
            RPCUtils.RegisterMethod(nameof(RPC_RequestSellItem), RPC_RequestSellItem);
        }

        [Server]
        public static void RPC_RequestTraderCoins(long sender, ZPackage _)
        {
            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestTraderCoins), BetterTraderServer.TraderInstance.Coins);
        }

        [Server]
        public static void RPC_RequestAvailablePurchaseItems(long sender, ZPackage _)
        {
            if (!inventoryRequests.ContainsKey(sender))
            {
                inventoryRequests.TryAdd(sender, null);
            }

            int requestID = DateTime.Now.ToString().GetStableHashCode();
            Thread senderThread = inventoryRequests[sender];

            if (senderThread == null || !senderThread.IsAlive)
            {
                var requestThread = new Thread(() =>
                {
                    Stack<ICirculatedItem> items = new Stack<ICirculatedItem>(BetterTraderServer.TraderInstance.GetItemsClientCanPurchase());
                    ISocket senderSocket = ZNet.instance.GetPeer(sender)?.m_socket;
                    int totalItemsToSend = items.Count;

                    while (items.Count > 0)
                    {
                        // TODO: Accessing "send queue size" in this way is not thread safe. Find workaround or alternative.
                        while (senderSocket != null && senderSocket.GetSendQueueSize() >= 20000)
                        {
                            Thread.Sleep(0);
                        }

                        var segmentedPackage = new ZPackage();
                        segmentedPackage.Write(requestID);
                        int packageSize = Math.Min(10, items.Count);
                        segmentedPackage.Write(totalItemsToSend);
                        segmentedPackage.Write(packageSize);

                        for (int i = 0; i < packageSize; i++)
                        {
                            ICirculatedItem item = items.Pop();
                            item.Serialize(ref segmentedPackage);
                        }

                        ThreadingUtils.ExecuteOnMainThread(() => RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestAvailablePurchaseItems), new object[] { segmentedPackage }));
                        Thread.Sleep(0);
                    }
                });

                requestThread.IsBackground = true;
                inventoryRequests[sender] = requestThread;
                requestThread.Start();
            }
        }

        [Server]
        public static void RPC_RequestAvailableSellItems(long sender, ZPackage pkg)
        {
            ZPackage playerInventoryPkg = pkg.ReadPackage();
            int playerInventoryItemCount = playerInventoryPkg.ReadInt();
            var responsePackage = new ZPackage();
            List<ICirculatedItem> items = new List<ICirculatedItem>();

            for (int i = 0; i < playerInventoryItemCount; i++)
            {
                var item = new CirculatedItem();
                item.Deserialize(ref playerInventoryPkg);
                items.Add(item);
            }

            BetterTraderServer.TraderInstance.GetItemsClientCanSell(items, ref responsePackage);
            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestAvailableSellItems), responsePackage);
        }

        [Server]
        public static void RPC_RequestPurchaseItem(long sender, ZPackage pkg)
        {
            string itemToPurchase = pkg.ReadString();
            int quantity = pkg.ReadInt();

            if (BetterTraderServer.TraderInstance.CanSell(itemToPurchase, quantity))
            {
                int purchasePrice = BetterTraderServer.TraderInstance.hashItemAssociations[itemToPurchase.GetStableHashCode()].Item1.CurrentPurchasePrice;
                BetterTraderServer.TraderInstance.PurchaseItem(itemToPurchase, quantity);
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestPurchaseItem), true, itemToPurchase, quantity, purchasePrice * quantity);
            }
            else
            {
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestPurchaseItem), false);
            }
        }

        [Server]
        public static void RPC_RequestSellItem(long sender, ZPackage pkg)
        {
            ZPackage requestPackage = pkg.ReadPackage();
            int quantity = requestPackage.ReadInt();
            ICirculatedItem inventoryItem = new CirculatedItem();
            inventoryItem.Deserialize(ref requestPackage);

            if (BetterTraderServer.TraderInstance.CanBeSold(quantity, inventoryItem))
            {
                var responsePackage = new ZPackage();
                ICirculatedItem equivalentTraderItem = BetterTraderServer.TraderInstance.GetItem(inventoryItem.Name);
                inventoryItem.CurrentSalesPrice = equivalentTraderItem.CurrentSalesPrice;
                responsePackage.Write(quantity);
                inventoryItem.Serialize(ref responsePackage);
                BetterTraderServer.TraderInstance.SellItem(inventoryItem.Name, quantity);
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestSellItem), true, responsePackage);
            }
            else
            {
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestSellItem), false);
            }
        }
    }
}
