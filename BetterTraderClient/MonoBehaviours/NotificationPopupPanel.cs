using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class NotificationPopupPanel : MonoBehaviour
    {
        [field: SerializeField]
        public TMP_Text TitleText { get; private set; }
        [field: SerializeField]
        public TMP_Text DescriptionText { get; private set; }
        private Action accept = () => { };
        private Action deny = () => { };

        public void Accept()
        {
            accept();
            Reset();
        }

        public void Deny()
        {
            deny();
            Reset();
        }

        public void Reset()
        {
            accept = () => { };
            deny = () => { };
            gameObject.SetActive(false);
        }

        public void Show(string title, string description, Action accept, Action deny)
        {
            TitleText.text = title;
            DescriptionText.text = description;
            this.accept = accept;
            this.deny = deny;
            gameObject.SetActive(true);
        }
    }
}
