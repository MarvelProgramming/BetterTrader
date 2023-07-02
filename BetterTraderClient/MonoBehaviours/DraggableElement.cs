using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class DraggableElement : MonoBehaviour, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private RectTransform rectTransform;
        private bool dragging;
        private Vector3 initialMousePosition;
        private Vector2 initialRectTransformPosition;

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                dragging = true;
                initialMousePosition = Input.mousePosition;
                initialRectTransformPosition = rectTransform.position;
            }

            Vector2 mouseDelta = initialMousePosition - Input.mousePosition;
            rectTransform.position = initialRectTransformPosition - new Vector2(mouseDelta.x, mouseDelta.y);

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;

            if (rectTransform.anchoredPosition.x < rectTransform.sizeDelta.x * 0.5f)
            {
                rectTransform.anchoredPosition += Vector2.right * Mathf.Abs(rectTransform.anchoredPosition.x - rectTransform.sizeDelta.x * 0.5f);
            }

            if (rectTransform.anchoredPosition.y < rectTransform.sizeDelta.y * 0.5f)
            {
                rectTransform.anchoredPosition += Vector2.up * Mathf.Abs(rectTransform.anchoredPosition.y - rectTransform.sizeDelta.y * 0.5f);
            }

            if (rectTransform.anchoredPosition.x > Mathf.Abs(rectTransform.sizeDelta.x) * 0.5f)
            {
                rectTransform.anchoredPosition += Vector2.left * (rectTransform.anchoredPosition.x - Mathf.Abs(rectTransform.sizeDelta.x) * 0.5f);
            }

            if (rectTransform.anchoredPosition.y > Mathf.Abs(rectTransform.sizeDelta.y) * 0.5f)
            {
                rectTransform.anchoredPosition += Vector2.down * (rectTransform.anchoredPosition.y - Mathf.Abs(rectTransform.sizeDelta.y) * 0.5f);
            }
        }
    }
}
