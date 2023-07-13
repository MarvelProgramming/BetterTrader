using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class DeselectEvent : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent events;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                List<RaycastResult> results = new List<RaycastResult>();
                var pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = Input.mousePosition;
                EventSystem.current.RaycastAll(pointerEventData, results);

                if (!results.Any(result => result.gameObject.name == gameObject.name))
                {
                    events.Invoke();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}
