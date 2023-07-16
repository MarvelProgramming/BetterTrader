using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using static ClutterSystem;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class TradingMenu : MonoBehaviour, IScrollHandler
    {
#pragma warning disable CS0649
        [field: SerializeField, Header("Base")]
        public CustomScrollView ItemListPanel { get; private set; }
        [field: SerializeField]
        public ItemPanel ItemPanelPrefab { get; private set; }
        [field: SerializeField]
        public GameObject TraderCoinsWrapper { get; private set; }
        [field: SerializeField]
        public GameObject RepairButtonsWrapper { get; private set; }
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
        public NotificationPopupPanel NotificationPopupPanelInstance { get; private set; }
        [field: SerializeField]
        public TMP_InputField TradeQuantityInput { get; private set; }
        [field: SerializeField]
        public TMP_InputField TradeItemFilterInput { get; private set; }
        [field: SerializeField]
        public CustomDropdown TradeItemFilterDropdown { get; private set; }
        [field: SerializeField]
        public GameObject TradeItemInspectionPrefab { get; private set; }
        private ItemInspector tradeItemInspector;

#pragma warning restore CS0649
        private float itemPanelHoverDelay = 0.5f;
        private float itemFilterDebounceTime = 0.4f;
        private string itemFilter = string.Empty;
        private bool canRepairItems;
        private int perItemRepairCost;
        private ItemPanel lastHoveredItemPanel;
        private ItemPanel lastSelectedItemPanel;
        private ICirculatedItem lastSelectedItem => lastSelectedItemPanel?.Item;
        private ItemDetailsPopupPanel itemDetailsPopupPanel;
        private readonly List<ICirculatedItem> traderInventoryItems = new List<ICirculatedItem>();
        private readonly List<ICirculatedItem> sellablePlayerInventoryItems = new List<ICirculatedItem>();
        private TradeMode tradeMode = TradeMode.Sell;
        private bool hasInitialized;
        [SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier
        private SortingManager sortingManager;
#pragma warning restore IDE0044 // Add readonly modifier

        public enum TradeMode
        {
            Buy,
            Sell
        }

        public void Hide()
        {
            StoreGui.instance.Hide();
        }

        public void SetBuyTradeMode()
        {
            Reset();
            tradeMode = TradeMode.Buy;
            UpdateMenu();
            RequestNewestData();
        }

        public void SetSellTradeMode()
        {
            Reset();
            tradeMode = TradeMode.Sell;
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

        public void OnRepairItem(bool repairAllItems)
        {
            if (!canRepairItems)
            {
                EventManager.RaiseNotification("Cannot Repair Item", "The trader is not repairing items at this time.", () => { }, () => { });
                return;
            }

            List<ItemDrop.ItemData> wornItems = new List<ItemDrop.ItemData>();
            Player.m_localPlayer.m_inventory.GetWornItems(wornItems);

            if (wornItems.Count == 0)
            {
                EventManager.RaiseNotification("No Reparable Items", "None of your items are in need of repair!", () => { }, () => { });
                return;
            }

            int repairCost = perItemRepairCost * (repairAllItems ? wornItems.Count : 1);
            
            if (TryGetPlayerCoinsTextAsInt(out int playerCoins))
            {
                if (playerCoins < repairCost)
                {
                    EventManager.RaiseNotification("Cannot Afford Repair", $"It costs {repairCost} to repair {wornItems.Count} item(s). You need more coins!", () => { }, () => { });
                }
                else
                {
                    RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestRepairItems), repairAllItems ? wornItems.Count : 1);
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

                if (lastSelectedItem != null)
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

                throw new Exception("Failed to update trade quantity!", e);
            }
        }

        public void OnItemFilterChanged(string inputFilter)
        {
            itemFilter = inputFilter;
            ItemListPanel.Reset();
            CancelInvoke(nameof(UpdateMenu));
            Invoke(nameof(UpdateMenu), itemFilterDebounceTime);
        }

        public void OnItemDropdownFilterChanged()
        {
            ItemListPanel.Reset();
            UpdateMenu();
        }

        public void OnScroll(PointerEventData eventData)
        {
            ItemListPanel.NudgeScrollValue(eventData.scrollDelta.y);
        }

        public void OnSortingModeChanged()
        {
            UpdateMenu();
        }

        private void UpdateTradeTotal()
        {
            if (lastSelectedItem != null && int.TryParse(TradeQuantityInput.text, out int currentTradeQuantity))
            {
                if (tradeMode  == TradeMode.Buy)
                {
                    TotalTradeValueText.text = $"{lastSelectedItem.CurrentPurchasePrice * currentTradeQuantity}c";
                }
                else
                {
                    TotalTradeValueText.text = $"{lastSelectedItem.CurrentSalesPrice * currentTradeQuantity}c";
                }
            }
            else
            {
                TotalTradeValueText.text = "0c";
            }
        }

        private void Awake()
        {
            EventManager.OnNotification += HandleNotification;
            EventManager.OnMousePointerEnterItemPanel += HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel += HandleMousePointerExitItemPanel;
            EventManager.OnMouseClickedItemPanel += HandleSelectedItemPanel;
            EventManager.OnInspectItemPanel += HandleInspectItemPanel;
            InitializeUI();
        }

        private void OnEnable()
        {
            if (!hasInitialized)
            {
                return;
            }

            EventManager.OnPlayerRepairedItems += HandlePlayerRepairedItems;
            EventManager.OnPlayerInventoryChanged += HandlePlayerInventoryChanged;
            EventManager.OnFetchedTraderInfo += HandleFetchedTraderInfo;
            EventManager.OnPlayerCoinsChanged += HandlePlayerCoinsChanged;
            EventManager.OnFetchedAvailablePurchaseItems += HandleFetchedAvailablePurchaseItems;
            EventManager.OnFetchedAvailableSellItems += HandleFetchedAvailableSellItems;

            if (Player.m_localPlayer)
            {
                Player.m_localPlayer.m_moveDir = Vector3.zero;
                Player.m_localPlayer.GetComponent<PlayerController>().enabled = false;
            }

            UpdateMenu();
            RequestNewestData();
        }

        private void OnDisable()
        {
            EventManager.OnPlayerRepairedItems -= HandlePlayerRepairedItems;
            EventManager.OnPlayerInventoryChanged -= HandlePlayerInventoryChanged;
            EventManager.OnFetchedTraderInfo -= HandleFetchedTraderInfo;
            EventManager.OnPlayerCoinsChanged -= HandlePlayerCoinsChanged;
            EventManager.OnFetchedAvailablePurchaseItems -= HandleFetchedAvailablePurchaseItems;
            EventManager.OnFetchedAvailableSellItems -= HandleFetchedAvailableSellItems;

            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.GetComponent<PlayerController>().enabled = true;
            }
            HandleMousePointerExitItemPanel();
            tradeItemInspector.RootPanel.SetActive(false);
            sortingManager.SetItemSortingState(SortingManager.SortingState.Off, false);
        }

        private void OnDestroy()
        {
            EventManager.OnNotification -= HandleNotification;
            EventManager.OnMousePointerEnterItemPanel -= HandleItemPanelHover;
            EventManager.OnMousePointerExitItemPanel -= HandleMousePointerExitItemPanel;
            EventManager.OnMouseClickedItemPanel -= HandleSelectedItemPanel;
            EventManager.OnInspectItemPanel -= HandleInspectItemPanel;
        }

        private void SetupItemDetailsPopupPanel()
        {
            itemDetailsPopupPanel.Setup(lastHoveredItemPanel);
            itemDetailsPopupPanel.CanvasGroup.alpha = 1;
        }

        private void InitializeUI()
        {
            itemDetailsPopupPanel = Instantiate(ItemDetailsPopupPanelPrefab, transform.parent);
            tradeItemInspector = Instantiate(TradeItemInspectionPrefab, transform.parent).GetComponentInChildren<ItemInspector>();
            Reset();

            // The menu is active with it's alpha set to 0 as the game is loading. This is so that it can
            // perform some of the more expensive operations at startup, instead of the user experiencing
            // a large lag spike when opening the menu.
            StartCoroutine(DelayedDisable());
        }

        private IEnumerator DelayedDisable()
        {
            yield return new WaitForEndOfFrame();
            gameObject.SetActive(false);
            GetComponent<CanvasGroup>().alpha = 1;
            hasInitialized = true;
        }

        private void Reset()
        {
            TradeItemFilterInput.SetTextWithoutNotify(string.Empty);
            TradeItemFilterDropdown.Reset();
            sortingManager.SetItemSortingState(SortingManager.SortingState.Off, false);
            TradeQuantityInput.SetTextWithoutNotify("1");
            TotalTradeValueText.text = "0c";
            itemFilter = string.Empty;

            if (lastSelectedItemPanel != null)
            {
                lastSelectedItemPanel.Deselect();
            }

            lastSelectedItemPanel = null;
            lastHoveredItemPanel = null;
            traderInventoryItems.Clear();
            sellablePlayerInventoryItems.Clear();
            NotificationPopupPanelInstance.Reset();
            ItemListPanel.Reset();
        }

        private void HandlePlayerRepairedItems()
        {
            UpdateMenu();
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

        private void HandleInspectItemPanel(ItemPanel panel)
        {
            if (!tradeItemInspector.RootPanel.activeSelf)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, tradeItemInspector.RootPanel.transform.position, null, out Vector2 localPoint))
                {
                    tradeItemInspector.RootPanel.transform.localPosition -= new Vector3(localPoint.x, localPoint.y, 0);
                }
            }

            tradeItemInspector.SetInspectedItem(panel.Item);
        }

        private void HandleFetchedTraderInfo(bool hasCoins, int coins, bool newCanRepairItems, int newPerItemRepairCost)
        {
            if (TraderCoinsWrapper != null)
            {
                TraderCoinsWrapper.SetActive(hasCoins);
            }

            TraderCoinsText.text = $"{coins}c";

            if (RepairButtonsWrapper != null)
            {
                RepairButtonsWrapper.SetActive(newCanRepairItems);
                canRepairItems = newCanRepairItems;
            }

            perItemRepairCost = newPerItemRepairCost;
        }

        private void HandlePlayerInventoryChanged()
        {
            UpdateTradeTotal();
            RequestNewestData();
        }

        private void HandleMousePointerExitItemPanel()
        {
            CancelInvoke(nameof(SetupItemDetailsPopupPanel));
            itemDetailsPopupPanel.CanvasGroup.alpha = 0;
        }

        private void HandleNotification(string title, string description, Action accept, Action deny)
        {
            NotificationPopupPanelInstance.Show(title, description, accept, deny);
        }

        private void HandleItemPanelHover(ItemPanel itemPanel)
        {
            lastHoveredItemPanel = itemPanel;
            Invoke(nameof(SetupItemDetailsPopupPanel), itemPanelHoverDelay);
        }

        private void HandlePlayerCoinsChanged(int coins)
        {
            PlayerCoinsText.text = $"{coins}c";
        }

        private void HandleFetchedAvailablePurchaseItems(List<ICirculatedItem> items)
        {
            traderInventoryItems.Clear();
            traderInventoryItems.AddRange(items);
            UpdateTradeItemFilterDropdownOptions(items);
            UpdateMenu();
        }

        private void HandleFetchedAvailableSellItems(List<ICirculatedItem> items)
        {
            sellablePlayerInventoryItems.Clear();
            sellablePlayerInventoryItems.AddRange(items);
            UpdateTradeItemFilterDropdownOptions(items);
            UpdateMenu();
        }

        private void UpdateMenu()
        {
            TradeButtonText.text = tradeMode.ToString();
            List<ICirculatedItem> targetCollection = tradeMode == TradeMode.Buy ? traderInventoryItems : sellablePlayerInventoryItems;
            List<ICirculatedItem> filteredItems = GetFilteredItemCollection(targetCollection);
            List<ICirculatedItem> sortedItems = GetSortedItemCollection(filteredItems);
            ItemListPanel.SetupItems(sortedItems, tradeMode);
            ItemListPanel.UpdateView();
        }

        private List<ICirculatedItem> GetFilteredItemCollection(List<ICirculatedItem> items)
        {
            List<ICirculatedItem> result;
            result = items.Where(item => 
            (string.IsNullOrEmpty(itemFilter) || Localization.instance.Localize(item.Drop.m_itemData.m_shared.m_name).ToLower().Replace(" ", "").Contains(itemFilter.ToLower().Replace(" ", ""))) && 
            (TradeItemFilterDropdown.GetBitField() & (int)Mathf.Pow(2, (int)item.Drop.m_itemData.m_shared.m_itemType)) != 0 &&
            (item.Drop.m_itemData.m_shared.m_dlc.Length == 0 || DLCMan.instance.IsDLCInstalled(item.Drop.m_itemData.m_shared.m_dlc))).ToList();

            return result;
        }

        private List<ICirculatedItem> GetSortedItemCollection(List<ICirculatedItem> items)
        {
            List<ICirculatedItem> result = new List<ICirculatedItem>(items);

            if (sortingManager.CurrentSortingState == SortingManager.SortingState.Off)
            {
                return result;
            }

            switch(sortingManager.CurrentSortingMode)
            {
                case SortingManager.SortingMode.Alphabetical:
                    result.Sort((firstItem, secondItem) =>
                    {
                        string sanitizedFirstItemName = Localization.instance.Localize(firstItem.Drop.m_itemData.m_shared.m_name).ToLower().Replace(" ", "");
                        string sanitizedSecondItemName = Localization.instance.Localize(secondItem.Drop.m_itemData.m_shared.m_name).ToLower().Replace(" ", "");

                        for (int i = 0; i < Mathf.Min(sanitizedFirstItemName.Length, sanitizedSecondItemName.Length); i++)
                        {
                            int firstNameCurCharVal = sanitizedFirstItemName[i];
                            int secondNameCurCharVal = sanitizedSecondItemName[i];

                            if (firstNameCurCharVal > secondNameCurCharVal)
                            {
                                return 1;
                            }
                            else if (firstNameCurCharVal < secondNameCurCharVal)
                            {
                                return -1;
                            }
                        }

                        return sanitizedFirstItemName.Length - sanitizedSecondItemName.Length;
                    });

                    break;
                case SortingManager.SortingMode.Stack:
                    result.Sort((firstItem, secondItem) =>
                    {
                        int sortVal = secondItem.CurrentStock - firstItem.CurrentStock;

                        return sortVal == 0 ? firstItem.Name.GetStableHashCode() - secondItem.Name.GetStableHashCode() : sortVal;
                    });

                    break;
                case SortingManager.SortingMode.Value:
                    result.Sort((firstItem, secondItem) =>
                    {
                        int firstItemValue = tradeMode == TradeMode.Buy ? firstItem.CurrentPurchasePrice : firstItem.CurrentSalesPrice;
                        int secondItemValue = tradeMode == TradeMode.Buy ? secondItem.CurrentPurchasePrice : secondItem.CurrentSalesPrice;
                        int sortVal = secondItemValue - firstItemValue;

                        return sortVal == 0 ? secondItem.Name.GetStableHashCode() - firstItem.Name.GetStableHashCode() : sortVal;
                    });

                    break;
            }

            if (sortingManager.CurrentSortingState == SortingManager.SortingState.Descending)
            {
                result.Reverse();
            }

            return result;
        }

        private void UpdateTradeItemFilterDropdownOptions(List<ICirculatedItem> items)
        {
            List<CustomDropdown.Option> dropdownOptions = new List<CustomDropdown.Option>();
            HashSet<ItemDrop.ItemData.ItemType> dropdownTypes = new HashSet<ItemDrop.ItemData.ItemType>();
            items.ForEach(item => dropdownTypes.Add(item.Drop.m_itemData.m_shared.m_itemType));
            // Using Mathf.Pow since Valheim's ItemDrop.ItemData.ItemType enum doesn't use powers of 2, causing bitfield comparisons to fail.
            dropdownTypes.ToList().ForEach(type => dropdownOptions.Add(new CustomDropdown.Option(type.ToString(), (int)Mathf.Pow(2, (int)type), true)));
            TradeItemFilterDropdown.SetOptions(dropdownOptions);
        }

        private void RequestNewestData()
        {
            RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestTraderInfo));

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

                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), nameof(RPC.RPC_RequestAvailableSellItems), requestPackage);
            }
        }

        private bool TryGetPlayerCoinsTextAsInt(out int playerCoins)
        {
            return int.TryParse(PlayerCoinsText.text.Substring(0, PlayerCoinsText.text.Length - 1), out playerCoins);
        }
    }
}
