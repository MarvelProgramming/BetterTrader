using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using System.Globalization;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class ItemDetailsPopupPanel : MonoBehaviour
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
            ItemNameText.SetText($"<b>{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Localization.instance.Localize(itemPanel.Item.Drop.GetHoverName()))}</b>");
            string tooltipText = Localization.instance.Localize(itemPanel.Item.Drop.m_itemData.GetTooltip());

            // Removing control characters so that they don't show up as text artifacts in the tooltip.
            tooltipText = new string(tooltipText.Where(c => !char.IsControl(c) || c == '\n').ToArray());
            ItemTooltipText.SetText(tooltipText);
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
            Utils.ClampUIToScreen(transform as RectTransform);
        }
    }
}
