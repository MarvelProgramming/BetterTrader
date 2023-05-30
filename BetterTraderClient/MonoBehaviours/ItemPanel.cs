using Menthus15Mods.Valheim.BetterTraderLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal class ItemPanel : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
    {
#pragma warning disable CS0649
        [field: SerializeField]
        public Image ItemIcon { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemNameText { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemValueText { get; private set; }
        [field: SerializeField]
        public GameObject IsSelectedDecoration { get; private set; }
#pragma warning restore CS0649
        public Item Item { get; private set; }

        public void SetupUI(Item item, TradingMenu.TradeMode tradeMode)
        {
            ItemIcon.sprite = item.Drop.m_itemData.GetIcon();
            ItemNameText.text = Localization.instance.Localize(item.Drop.GetHoverName());
            ItemValueText.text = (tradeMode == TradingMenu.TradeMode.Buy ? item.PurchasePrice.ToString() : item.SalesPrice.ToString()) + "c";
            Item = item;
        }

        public void Deselect()
        {
            IsSelectedDecoration.SetActive(false);
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
