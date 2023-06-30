using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class DropdownOption : MonoBehaviour
    {
        [field: SerializeField]
        public Image CheckedDecoration { get; private set; }
        [field: SerializeField]
        public TMP_Text Label { get; private set; }
        [field: SerializeField]
        public Button SelectionButton { get; private set; }
    }
}
