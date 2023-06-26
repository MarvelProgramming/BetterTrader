using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class TradingMenu : MonoBehaviour
    {
#pragma warning disable CS0649
        [field: SerializeField]
        public Transform ItemListPanel { get; private set; }
        [field: SerializeField]
        public ItemPanel ItemPanelPrefab { get; private set; }
        [field: SerializeField]
        public TMP_Text TraderCoinsText { get; private set; }
        [field: SerializeField]
        public TMP_Text PlayerCoinsText { get; private set; }
        [field: SerializeField]
        public TMP_Text TradeButtonText { get; private set; }
        [field: SerializeField]
        public TMP_Text TotalTradeValueText { get; private set; }
        [field: SerializeField]
        public ItemDetailsPopupPanel ItemDetailsPopupPanelPrefab { get; private set; }
        [field: SerializeField]
        public NotificationPopupPanel NotificationPopupPanelPrefab { get; private set; }
        [field: SerializeField]
        public TMP_InputField TradeQuantityInput { get; private set; }
#pragma warning restore CS0649
        private float itemPanelHoverDelay = 0.5f;
        private ItemPanel lastHoveredItemPanel;
        private ItemPanel lastSelectedItemPanel;
        private ICirculatedItem lastSelectedItem => lastSelectedItemPanel?.Item;
        private ItemDetailsPopupPanel itemDetailsPopupPanel;
        private NotificationPopupPanel notificationPopupPanel;
        private readonly List<ICirculatedItem> traderInventoryItems = new List<ICirculatedItem>();
        private readonly List<ICirculatedItem> sellablePlayerInventoryItems = new List<ICirculatedItem>();
        private TradeMode tradeMode = TradeMode.Sell;
        public enum TradeMode
        {
            Buy,
            Sell
        }
        
        public void SetBuyTradeMode()
        {
            tradeMode = TradeMode.Buy;
            traderInventoryItems.Clear();
            UpdateMenu();
            RequestNewestData();
        }

        public void SetSellTradeMode()
        {
            tradeMode = TradeMode.Sell;
            sellablePlayerInventoryItems.Clear();
            UpdateMenu();
            RequestNewestData();
        }

        public void SubmitTrade()
        {
            if (lastSelectedItemPanel == null)
            {
                return;
            }

            ICirculatedItem tradeItem = lastSelectedItem;

            if (int.TryParse(TradeQuantityInput.text, out int tradeQuantity)) {
                if (tradeMode == TradeMode.Buy)
                {
                    if (Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name) < tradeItem.CurrentPurchasePrice * tradeQuantity)
                    {
                        EventManager.RaiseNotification("Unsuccessful Trade", "You don't have enough coins to complete the purchase!", () => { }, () => { });

                        return;
                    }

                    if (!Player.m_localPlayer.m_inventory.CanAddItemStack(tradeItem.Drop.m_itemData, tradeQuantity))
                    {
                        EventManager.RaiseNotification("Unsuccessful Trade", "You don't have enough space in your inventory to complete the purchase!", () => { }, () => { });

                        return;
                    }

                    RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestPurchaseItem),
                            lastSelectedItem.Name,
                            tradeQuantity);
                }
                else
                {
                    var requestPackage = new ZPackage();
                    requestPackage.Write(tradeQuantity);
                    var circulatedItem = new CirculatedItem(lastSelectedItem.Name, lastSelectedItem.GridPosition);
                    circulatedItem.Serialize(ref requestPackage);

                    if (lastSelectedItem.IsEquipped)
                    {
                        EventManager.RaiseNotification("Selling Equipped Item", "The item you've chosen to sell is currently equipped. Are you sure you want to sell it?",
                            () =>
                            {
                                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestSellItem),
                                requestPackage);
                            },
                            () => { });
                    }
                    else
                    {
                        RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestSellItem),
                        requestPackage);
                    }
                }
            }
        }

        public void OnTradeQuantityChanged(string inputQuantity)
        {
            try
            {
                int quantity;

                if (string.IsNullOrEmpty(inputQuantity))
                {
                    if (!TradeQuantityInput.isFocused)
                    {
                        quantity = 1;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    quantity = int.Parse(inputQuantity);
                }

                if (quantity < 1)
                {
                    quantity = 1;
                }

                if (lastSelectedItemPanel != null)
                {
                    quantity = Mathf.Min(quantity, lastSelectedItem.CurrentStock);
                }

                TradeQuantityInput.SetTextWithoutNotify(quantity.ToString());
                UpdateTradeTotal();
            }
            catch(Exception e)
            {
                if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                {
                    throw new Exception("Failed to convert trade quantity input to int (number)! Make sure the \"Content Type\" option is set to \"Integer Number\" for the Unity input", e);
                }

                throw e;
            }
        }

        private void UpdateTradeTotal()
        {
            if (int.TryParse(TradeQuantityInput.text, out int currentTradeQuantity))
            {
                if (tradeMode  == TradeMode.Buy)
                {
                    TotalTradeValueText.text = $"total: {lastSelectedItem.CurrentPurchasePrice * currentTradeQuantity}c";
                }
                else
                {
                    TotalTradeValueText.text = $"total: {lastSelectedItem.CurrentSalesPrice * currentTradeQuantity}c";
                }
            }
            else
            {
                TotalTradeValueText.text = "total: 0c";
            }
        }

        private void Awake()
        {
            EventManager.OnNotification += HandleNotification;
            EventManager.OnMousePointerEnterItemPanel += HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel += HandleMousePointerExitItemPanel;
            EventManager.OnMouseClickedItemPanel += HandleSelectedItemPanel;
            InitializePanels();
        }

        private void Start()
        {
            Reset();
        }

        private void OnEnable()
        {
            UpdateMenu();
            EventManager.OnPlayerInventoryChanged += HandlePlayerInventoryChanged;
            EventManager.OnPlayerCoinsChanged += HandlePlayerCoinsChanged;
            EventManager.OnFetchedTraderCoins += HandleFetchedTraderCoins;
            EventManager.OnFetchedAvailablePurchaseItems += HandleFetchedAvailablePurchaseItems;
            EventManager.OnFetchedAvailableSellItems += HandleFetchedAvailableSellItems;
            Reset();
            RequestNewestData();
        }

        private void OnDisable()
        {
            EventManager.OnPlayerInventoryChanged -= HandlePlayerInventoryChanged;
            EventManager.OnPlayerCoinsChanged -= HandlePlayerCoinsChanged;
            EventManager.OnFetchedTraderCoins -= HandleFetchedTraderCoins;
            EventManager.OnFetchedAvailablePurchaseItems -= HandleFetchedAvailablePurchaseItems;
            EventManager.OnFetchedAvailableSellItems -= HandleFetchedAvailableSellItems;
            notificationPopupPanel.Reset();
            HandleMousePointerExitItemPanel();
        }

        private void OnDestroy()
        {
            EventManager.OnNotification -= HandleNotification;
            EventManager.OnMousePointerEnterItemPanel -= HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel -= HandleMousePointerExitItemPanel;
            EventManager.OnMouseClickedItemPanel -= HandleSelectedItemPanel;
        }

        private void SetupItemDetailsPopupPanel()
        {
            itemDetailsPopupPanel.Setup(lastHoveredItemPanel);
            itemDetailsPopupPanel.CanvasGroup.alpha = 1;
        }

        private void InitializePanels()
        {
            itemDetailsPopupPanel = Instantiate(ItemDetailsPopupPanelPrefab, transform.parent);
            notificationPopupPanel = Instantiate(NotificationPopupPanelPrefab, transform.parent);
        }

        private void HandleSelectedItemPanel(ItemPanel newlySelectedItemPanel)
        {
            if (lastSelectedItemPanel != null && newlySelectedItemPanel != lastSelectedItemPanel)
            {
                lastSelectedItemPanel.Deselect();
            }

            lastSelectedItemPanel = newlySelectedItemPanel;

            OnTradeQuantityChanged(TradeQuantityInput.text);
            UpdateTradeTotal();
        }

        private void Reset()
        {
            TradeQuantityInput.text = "1";
            TotalTradeValueText.text = "total: 0c";
            lastSelectedItemPanel = null;
            lastHoveredItemPanel = null;
        }

        private void HandlePlayerInventoryChanged()
        {
            RequestNewestData();
        }

        private void HandleMousePointerExitItemPanel()
        {
            CancelInvoke(nameof(SetupItemDetailsPopupPanel));
            itemDetailsPopupPanel.CanvasGroup.alpha = 0;
        }

        private void HandleNotification(string title, string description, Action accept, Action deny)
        {
            notificationPopupPanel.Show(title, description, accept, deny);
        }

        private void HandleItemPanelHover(ItemPanel itemPanel)
        {
            lastHoveredItemPanel = itemPanel;
            Invoke(nameof(SetupItemDetailsPopupPanel), itemPanelHoverDelay);
        }

        private void HandlePlayerCoinsChanged(int coins)
        {
            PlayerCoinsText.text = $"my coins: {coins}c";
        }

        private void HandleFetchedTraderCoins(int coins)
        {
            TraderCoinsText.text = $"{coins}c";
        }

        private void HandleFetchedAvailablePurchaseItems(List<ICirculatedItem> items)
        {
            traderInventoryItems.Clear();
            traderInventoryItems.AddRange(items);
            UpdateMenu();
        }

        private void HandleFetchedAvailableSellItems(List<ICirculatedItem> items)
        {
            sellablePlayerInventoryItems.Clear();
            sellablePlayerInventoryItems.AddRange(items);
            UpdateMenu();
        }

        private void UpdateMenu()
        {
            TradeButtonText.text = tradeMode.ToString();
            ClearTransformChildren(ItemListPanel);

            if (tradeMode == TradeMode.Buy)
            {
                PopulateWithTraderInventoryItems();
            }
            else
            {
                PopulateWithPlayerInventoryItems();
            }
        }

        private void PopulateWithTraderInventoryItems()
        {
            traderInventoryItems.ForEach(inventoryItem =>
            {
                var itemPanel = Instantiate(ItemPanelPrefab, ItemListPanel);
                itemPanel.SetupUI(inventoryItem, tradeMode);
            });
        }

        private void PopulateWithPlayerInventoryItems()
        {
            sellablePlayerInventoryItems.ForEach(inventoryItem =>
            {
                var itemPanel = Instantiate(ItemPanelPrefab, ItemListPanel);
                itemPanel.SetupUI(inventoryItem, tradeMode);
            });
        }

        private void ClearTransformChildren(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void RequestNewestData()
        {
            if (tradeMode == TradeMode.Buy)
            {
                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestAvailablePurchaseItems));
            }
            else
            {
                var requestPackage = new ZPackage();
                requestPackage.Write(Player.m_localPlayer.m_inventory.m_inventory.Count);

                Player.m_localPlayer.m_inventory.GetAllItems().ForEach(item =>
                {
                    var circulatedItem = new CirculatedItem(item.m_dropPrefab.name, false, false, false, Player.m_localPlayer.IsItemEquiped(item), item.m_gridPos, 100, -1, item.m_stack);
                    circulatedItem.Serialize(ref requestPackage);
                });

                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestAvailableSellItems),
                    requestPackage
                    );
            }
        }
    }
}
