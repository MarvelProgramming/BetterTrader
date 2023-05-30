using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Vector3 positionOffset = new Vector3(20, 0, 0);

        public void Setup(ItemPanel itemPanel)
        {
            ItemIcon.sprite = itemPanel.ItemIcon.sprite;
            ItemNameText.SetText($"<b>{Localization.instance.Localize(itemPanel.Item.Drop.GetHoverName())}</b>");
            ItemTooltipText.SetText($"{Localization.instance.Localize(itemPanel.Item.Drop.m_itemData.GetTooltip())}");
            UpdatePosition();
        }

        private void OnEnable()
        {
            ForceUpdateRectTransform();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void FixedUpdate()
        {
            UpdatePosition();
        }

        // TODO: See if a solution exists that doesn't introduce jitter
        // This "fixes" an issue where the popup panel wouldn't update its dimensions after inner content changes.
        // The tradeoff is an unappealing jitter whenever the popup is enabled
        private void ForceUpdateRectTransform()
        {
            GetComponentsInChildren<ContentSizeFitter>().ToList().ForEach(contentSizeFitter =>
            {
                contentSizeFitter.enabled = false;
                contentSizeFitter.enabled = true;
            });

            var thisContentSizeFitter = GetComponent<ContentSizeFitter>();
            thisContentSizeFitter.enabled = false;
            thisContentSizeFitter.enabled = true;

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        private void UpdatePosition()
        {
            transform.position = ZInput.instance.Input_mousePosition + positionOffset;
        }
    }
}
