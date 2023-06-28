using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class CustomScrollView : MonoBehaviour
    {
        [field: SerializeField]
        public RectTransform Content { get; private set; }
        [field: SerializeField]
        public Scrollbar Scrollbar { get; private set; }
        [field: SerializeField]
        public ItemPanel ItemPanelPrefab { get; private set; }
        [field: SerializeField]
        public int PaddedPanels;
        [field: SerializeField]
        public float Spacing { get; private set; }
        private int selectedItemPanelIndex = -1;
        private float panelHeight;
        private float panelHeightRemainder;
        private float itemPanelHeight;
        private float itemPanelSpaceOccupancy;
        private float accumulitiveItemPanelHeight;
        private int panelCount;
        private RectTransform rectTransform;
        private readonly List<ItemPanel> panels = new List<ItemPanel>();
        private readonly Dictionary<ItemPanel, RectTransform> panelRects = new Dictionary<ItemPanel, RectTransform>();
        private List<ICirculatedItem> items = new List<ICirculatedItem>();
        private TradingMenu.TradeMode tradeMode;

        public void NudgeScrollValue(float direction)
        {
            // Keeps the stepSize consistent regardless of the number of items in the list.
            float stepSize = panelHeight / itemPanelHeight / 5f / Mathf.Max(items.Count - panelCount, 1);
            Scrollbar.value = Mathf.Clamp(Scrollbar.value + (direction < 0 ? stepSize : -stepSize), 0, 1);
        }

        public void SetupItems(List<ICirculatedItem> items, TradingMenu.TradeMode tradeMode)
        {
            this.tradeMode = tradeMode;
            this.items = items;
            Scrollbar.value = 0;
            UpdateScrollHandleSize();
        }

        public void UpdateView()
        {
            UpdatePanels();
        }

        private void UpdatePanels()
        {
            float baseItemPanelPosition = GetBaseItemPanelPosition();

            for(int i = 0; i < panels.Count; i++)
            {
                ItemPanel panel = panels[i];
                float panelOffset = GetItemPanelOffset(i);
                float newPosition = baseItemPanelPosition - panelOffset;
                int overflowCount = GetOverflowCount(i);
                panelRects[panel].anchoredPosition = new Vector3(0, newPosition - overflowCount * accumulitiveItemPanelHeight, panel.transform.localPosition.z);
                panel.gameObject.SetActive(i < items.Count);
                int itemIndex = GetItemIndex(i);

                if (itemIndex < items.Count)
                {
                    panel.IsSelectedDecoration.SetActive(selectedItemPanelIndex == itemIndex);
                    panel.SetupUI(items[itemIndex], tradeMode);
                }
            }
        }

        private float GetBaseItemPanelPosition()
        {
            return Scrollbar.value * Math.Max(itemPanelSpaceOccupancy * (items.Count - panels.Count + 1) + panelHeight % itemPanelSpaceOccupancy - Spacing, 0);
        }

        private float GetItemPanelOffset(int panelIndex)
        {
            return itemPanelSpaceOccupancy * panelIndex;
        }

        private int GetOverflowCount(int panelIndex)
        {
            float panelOverflowPoint = Mathf.Max(accumulitiveItemPanelHeight - (GetItemPanelOffset(panelIndex) + itemPanelSpaceOccupancy) + GetBaseItemPanelPosition(), 0);
            int overflowCount = panelOverflowPoint == 0 ? 0 : Mathf.FloorToInt(panelOverflowPoint / accumulitiveItemPanelHeight);

            return overflowCount;
        }

        private int GetItemIndex(int panelIndex)
        {
            return panelIndex + GetOverflowCount(panelIndex) * panelCount;
        }

        private void Awake()
        {
            // Using coroutine to delay initialization until the second frame. That way the RectTransforms have a chance
            // to update and give the correct values.
            StartCoroutine(InitializeVariables());
        }

        private void OnEnable()
        {
            EventManager.OnMouseClickedItemPanel += HandleMouseClickItemPanel;
        }

        private void OnDisable()
        {
            EventManager.OnMouseClickedItemPanel -= HandleMouseClickItemPanel;
        }

        private void HandleMouseClickItemPanel(ItemPanel panel)
        {
            selectedItemPanelIndex = GetItemIndex(panel.transform.GetSiblingIndex());
        }

        private IEnumerator InitializeVariables()
        {
            yield return new WaitForEndOfFrame();
            rectTransform = Content.GetComponent<RectTransform>();
            panelHeight = rectTransform.rect.height;
            itemPanelHeight = ItemPanelPrefab.GetComponent<RectTransform>().sizeDelta.y;
            itemPanelSpaceOccupancy = itemPanelHeight + Spacing;
            panelHeightRemainder = panelHeight % itemPanelSpaceOccupancy;
            panelCount = Mathf.CeilToInt(panelHeight / itemPanelSpaceOccupancy) + PaddedPanels;
            accumulitiveItemPanelHeight = itemPanelSpaceOccupancy * panelCount;
            SetupItemPanelPool();
            UpdateScrollHandleSize();
            UpdateView();
        }

        private void SetupItemPanelPool()
        {
            for(int i = 0; i < panelCount; i++)
            {
                ItemPanel panel = Instantiate(ItemPanelPrefab, Vector3.zero, Quaternion.identity, Content);
                RectTransform panelRectTransform = panel.GetComponent<RectTransform>();
                panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width);
                panelRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelRectTransform.sizeDelta.y);
                panelRectTransform.anchoredPosition = Vector3.zero;
                panel.gameObject.SetActive(false);
                panels.Add(panel);
                panelRects.Add(panel, panelRectTransform);
            }
        }

        private void UpdateScrollHandleSize()
        {
            Scrollbar.size = Math.Min(items.Count == 0 ? 1 : panelCount / (items.Count + 1) - (panelHeightRemainder > 0 ? panelHeightRemainder / panelHeight : 0), 1);
        }
    }
}
