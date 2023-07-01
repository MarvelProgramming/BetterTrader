using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class ScalableElement : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private RectTransform rectTransform;
        [SerializeField]
        private RectTransform.Axis axis;
        [SerializeField]
        private float maxScale;
        [SerializeField]
        private float minScale;
        private bool dragging;
        private Vector3 initialMousePosition;
        private Vector3 initialRectTransformPosition;
        private Vector2 initialRectTransformSize;

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                dragging = true;
                initialMousePosition = Input.mousePosition;
                initialRectTransformPosition = rectTransform.anchoredPosition;
                initialRectTransformSize = rectTransform.rect.size;
            }

            Vector2 mouseDelta = initialMousePosition - Input.mousePosition;
            rectTransform.SetSizeWithCurrentAnchors(axis, axis == RectTransform.Axis.Horizontal ? initialRectTransformSize.x - mouseDelta.x : initialRectTransformSize.y + mouseDelta.y);

            if (axis == RectTransform.Axis.Horizontal)
            {
                if (rectTransform.rect.size.x < minScale)
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, minScale);
                }
                else if (rectTransform.rect.size.x > maxScale)
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, maxScale);
                }

                rectTransform.anchoredPosition = initialRectTransformPosition - Vector3.right * (initialRectTransformSize.x - rectTransform.rect.size.x) * 0.5f;
            }
            else
            {
                if (rectTransform.rect.size.y < minScale)
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, minScale);
                }
                else if (rectTransform.rect.size.y > maxScale)
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, maxScale);
                }

                rectTransform.anchoredPosition = initialRectTransformPosition - Vector3.down * (initialRectTransformSize.y - rectTransform.rect.size.y) * 0.5f;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }
    }
}
