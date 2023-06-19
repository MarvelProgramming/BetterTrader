using Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal class TradingMenu : MonoBehaviour
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
        public ItemDetailsPopupPanel ItemDetailsPopupPanelPrefab { get; private set; }
#pragma warning restore CS0649
        private float itemPanelHoverDelay = 0.5f;
        private ItemPanel lastHoveredItemPanel;
        private ItemPanel lastSelectedItemPanel;
        private ItemDetailsPopupPanel itemDetailsPopupPanel;
        private readonly List<ICirculatedItem> traderInventoryItems = new List<ICirculatedItem>();
        private TradeMode tradeMode = TradeMode.Sell;
        public enum TradeMode
        {
            Buy,
            Sell
        }

        public void SetBuyTradeMode()
        {
            tradeMode = TradeMode.Buy;
            RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestTraderInventory));
        }

        public void SetSellTradeMode()
        {
            tradeMode = TradeMode.Sell;
            UpdateMenu();
        }

        private void Awake()
        {
            EventManager.OnMousePointerEnterItemPanel += HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel += DisableItemDetailsPopupPanel;
            EventManager.OnMouseClickedItemPanel += UpdateSelectedItemPanel;
            InitializeItemDetailsPopupPanel();
        }

        private void OnEnable()
        {
            UpdateMenu();
            EventManager.OnFetchedTraderInfo += HandleFetchedTraderInfo;
        }

        private void OnDisable()
        {
            EventManager.OnFetchedTraderInfo -= HandleFetchedTraderInfo;
            DisableItemDetailsPopupPanel();
        }

        private void OnDestroy()
        {
            EventManager.OnMousePointerEnterItemPanel -= HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel -= DisableItemDetailsPopupPanel;
            EventManager.OnMouseClickedItemPanel -= UpdateSelectedItemPanel;
        }

        private void HandleItemPanelHover(ItemPanel itemPanel)
        {
            lastHoveredItemPanel = itemPanel;
            Invoke(nameof(SetupItemDetailsPopupPanel), itemPanelHoverDelay);
        }

        private void UpdateSelectedItemPanel(ItemPanel newlySelectedItemPanel)
        {
            if (lastSelectedItemPanel != null && newlySelectedItemPanel != lastSelectedItemPanel)
            {
                lastSelectedItemPanel.Deselect();
            }

            lastSelectedItemPanel = newlySelectedItemPanel;
        }

        private void SetupItemDetailsPopupPanel()
        {
            itemDetailsPopupPanel.Setup(lastHoveredItemPanel);
            itemDetailsPopupPanel.CanvasGroup.alpha = 1;
        }

        private void DisableItemDetailsPopupPanel()
        {
            CancelInvoke(nameof(SetupItemDetailsPopupPanel));
            itemDetailsPopupPanel.CanvasGroup.alpha = 0;
        }

        private void InitializeItemDetailsPopupPanel()
        {
            itemDetailsPopupPanel = Instantiate(ItemDetailsPopupPanelPrefab, transform.parent);
        }

        private void HandleFetchedTraderInfo(int coins, List<ICirculatedItem> items)
        {
            TraderCoinsText.text = coins.ToString() + "c";
            traderInventoryItems.Clear();
            traderInventoryItems.AddRange(items);

            if (tradeMode == TradeMode.Buy)
            {
                UpdateMenu();
            }
        }

        private void UpdateMenu()
        {
            TradeButtonText.text = tradeMode.ToString();
            ClearTransformChildren(ItemListPanel);

            switch (tradeMode)
            {
                case TradeMode.Buy:
                    PopulateWithTraderInventoryItems();
                    break;
                case TradeMode.Sell:
                    PopulateWithPlayerInventoryItems();
                    break;
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
            Player.m_localPlayer.m_inventory.GetAllItems().ForEach(inventoryItem =>
            {
                var itemPanel = Instantiate(ItemPanelPrefab, ItemListPanel);
                var item = new CirculatedItem(inventoryItem.m_dropPrefab.name);
                itemPanel.SetupUI(item, tradeMode);
            });
        }

        private void ClearTransformChildren(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
