using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    internal class ItemDetailsPopupPanel : MonoBehaviour
    {
        [field: SerializeField]
        public Image ItemIcon { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemNameText { get; private set; }
        [field: SerializeField]
        public TMP_Text ItemTooltipText { get; private set; }
        [field: SerializeField]
        public CanvasGroup CanvasGroup { get; private set; }
        private Vector3 positionOffset = new Vector3(20, 0, 0);

        public void Setup(ItemPanel itemPanel)
        {
            ItemIcon.sprite = itemPanel.ItemIcon.sprite;
            ItemNameText.SetText($"<b>{Localization.instance.Localize(itemPanel.Item.Drop.GetHoverName())}</b>");
            ItemTooltipText.SetText($"{Localization.instance.Localize(itemPanel.Item.Drop.m_itemData.GetTooltip())}");
            RefreshLayout();
            UpdatePosition();
        }

        private void OnGUI()
        {
            UpdatePosition();
        }

        private void RefreshLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ItemTooltipText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ItemTooltipText.transform.parent as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        private void UpdatePosition()
        {
            transform.position = ZInput.instance.Input_mousePosition + positionOffset;
        }
    }
}
