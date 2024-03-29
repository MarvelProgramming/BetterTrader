﻿using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class ItemPanel : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable CS0649
        [field: SerializeField]
        public Image ItemIcon { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemNameText { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemValueText { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemQuantityText { get; private set; }
        [field: SerializeField]
        public GameObject IsSelectedDecoration { get; private set; }
        [field: SerializeField]
        public GameObject IsEquippedDecoration { get; private set; }
        [field: SerializeField]
        public CanvasGroup CanvasGroup { get; private set; }
#pragma warning restore CS0649
        public ICirculatedItem Item { get; private set; }

        public void SetupUI(ICirculatedItem item, TradingMenu.TradeMode tradeMode)
        {
            ItemIcon.sprite = item.Drop.m_itemData.GetIcon();
            ItemNameText.text = Localization.instance.Localize(item.Drop.GetHoverName()).Split(' ').ToList().Select(segment => segment[0].ToString().ToUpper() + segment.Substring(1, segment.Length - 1)).Aggregate((acc, nextSegment) => acc + " " + nextSegment);
            ItemValueText.text = (tradeMode == TradingMenu.TradeMode.Buy ? item.CurrentPurchasePrice.ToString() : item.CurrentSalesPrice.ToString()) + "c";
            ItemQuantityText.text = $"x{item.CurrentStock}";
            IsEquippedDecoration.SetActive(tradeMode == TradingMenu.TradeMode.Sell && item.IsEquipped);
            Item = item;
            CanvasGroup.alpha = tradeMode == TradingMenu.TradeMode.Sell || Player.m_localPlayer.m_inventory.CountItems(StoreGui.instance.m_coinPrefab.m_itemData.m_shared.m_name) >= Item.CurrentPurchasePrice ? 1 : 0.4f;
        }

        public void Deselect()
        {
            IsSelectedDecoration.SetActive(false);
        }

        public void Inspect()
        {
            EventManager.OnInspectItemPanel(this);
        }

        public void OnSelect(BaseEventData eventData)
        {
            IsSelectedDecoration.SetActive(true);
            EventManager.OnMouseClickedItemPanel(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventManager.RaiseMousePointerEnterItemPanel(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EventManager.RaiseMousePointerExitItemPanel();
        }
    }
}
