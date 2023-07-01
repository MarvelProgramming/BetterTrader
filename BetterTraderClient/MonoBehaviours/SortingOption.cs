using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class SortingOption : MonoBehaviour, IPointerClickHandler
    {
        [field: SerializeField]
        public TMP_Text StateDecoration { get; private set; }
        public UnityEvent OnClick { get; private set; } = new UnityEvent();

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke();
        }
    }
}
