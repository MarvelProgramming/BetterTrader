using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours.CustomDropdown;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class CustomDropdown : MonoBehaviour
    {
        [field: SerializeField]
        public Transform ContentPanel { get; private set; }
        [field: SerializeField]
        public Selectable DropdownPanel { get; private set; }
        [field: SerializeField]
        public DropdownOption OptionUIPrefab { get; private set; }
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField]
        private UnityEvent OnChanged;
#pragma warning restore IDE0044 // Add readonly modifier
        private readonly List<Option> options = new List<Option>()
        { 
            new Option("None", 0),
            new Option("All", 1, true)
        };
        private readonly Dictionary<Option, DropdownOption> optionUIAssociations = new Dictionary<Option, DropdownOption>();
        private const float toggleDebounceTime = 0.2f;
        private bool canToggle = true;

        [Serializable]
        public class Option
        {
            public readonly string label;
            [HideInInspector]
            public int value;
            public bool isChecked;

            public Option(string label, int value)
            {
                this.label = label;
                this.value = value;
            }

            public Option(string label, int value, bool isChecked) : this(label, value)
            {
                this.isChecked = isChecked;
            }
        }

        public void Toggle()
        {
            if (!canToggle)
            {
                return;
            }

            DropdownPanel.gameObject.SetActive(!DropdownPanel.gameObject.activeSelf);
            canToggle = false;
            Invoke(nameof(EnableToggling), toggleDebounceTime);
        }

        public void OnDropdownPanelDeselected()
        {
            Toggle();
        }

        public void Apply()
        {
            OnChanged.Invoke();
        }

        public void SetOptions(List<Option> additionalOptions)
        {
            // Resetting "All" option before appending new flags.
            options[1].value = 0;
            options.RemoveRange(2, options.Count - 2);

            additionalOptions.ForEach(option =>
            {
                string label = option.label;

                if (label.ToLower() == "none" || label.ToLower() == "all")
                {
                    return;
                }

                options.Add(option);
                options[1].value |= option.value;
            });

            InitializeUI();
        }

        public int GetBitField()
        {
            int bitField = 0;

            foreach (Option option in options)
            {
                bitField |= option.isChecked ? option.value : 0;
            }

            return bitField;
        }

        public void Reset()
        {
            if (optionUIAssociations.Count == 0)
            {
                return;
            }

            OnDropdownOptionSelected(options[1]);
        }

        private void OnValidate()
        {
            if (!options.Any(option => option.label == "None"))
            {
                options.Insert(0, new Option("None", 0));
            }

            if (!options.Any(option => option.label == "All"))
            {
                options.Insert(1, new Option("All", 1));
            }
        }

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            optionUIAssociations.Clear();

            foreach (Transform child in ContentPanel)
            {
                Destroy(child.gameObject);
            }

            foreach (Option option in options)
            {
                DropdownOption optionUI = Instantiate(OptionUIPrefab, ContentPanel);
                optionUI.Label.text = option.label;
                optionUI.CheckedDecoration.gameObject.SetActive(option.isChecked);
                optionUI.SelectionButton.onClick.AddListener(() =>
                {
                    OnDropdownOptionSelected(option);
                });
                optionUIAssociations.Add(option, optionUI);
            }
        }

        private void OnDropdownOptionSelected(Option option)
        {
            SetOptionChecked(option, !option.isChecked);

            if (option.label == "None" && option.isChecked)
            {
                for(int i = 1; i < options.Count; i++)
                {
                    Option iOption = options[i];
                    SetOptionChecked(iOption, false);
                }
            }
            else if (option.label == "All" && option.isChecked)
            {
                SetOptionChecked(options[0], false);

                for(int i = 1; i < options.Count; i++)
                { 
                    Option iOption = options[i];
                    SetOptionChecked(iOption, true);
                }
            }

            SetOptionChecked(options[0], options.All(option => option.label == "None" || !option.isChecked));
            SetOptionChecked(options[1], options.All(option => (option.label == "None" && !option.isChecked) || option.label == "All" || option.isChecked));
            Apply();
        }

        private void SetOptionChecked(Option option, bool isChecked)
        {
            option.isChecked = isChecked;
            optionUIAssociations[option].CheckedDecoration.gameObject.SetActive(isChecked);
        }

        private void EnableToggling()
        {
            canToggle = true;
        }
    }
}
