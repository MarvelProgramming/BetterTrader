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
            Utils.ClampUIToScreen(rectTransform);
        }
    }
}
