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
using static Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours.SortingManager;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class CustomDropdown : MonoBehaviour, ISerializationCallbackReceiver
    {
        [field: SerializeField]
        public Transform ContentPanel { get; private set; }
        [field: SerializeField]
        public Selectable DropdownPanel { get; private set; }
        [field: SerializeField]
        public TMP_Text LabelText { get; private set; }
        [field: SerializeField]
        public bool IncludeGlobalOptions { get; private set; } = true;
        [field: SerializeField]
        public bool Multiselect { get; private set; } = true;
        [field: SerializeField]
        public DropdownOption OptionUIPrefab { get; private set; }
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField]
        private UnityEvent<int> OnChanged;
#pragma warning restore IDE0044 // Add readonly modifier
        [SerializeField]
        private Option[] options;
        [SerializeField, HideInInspector]
        private bool[] _optionIsCheckedStates;
        [SerializeField, HideInInspector]
        private string[] _optionLabels;
        [SerializeField, HideInInspector]
        private int[] _optionValues;
        private readonly Dictionary<Option, DropdownOption> optionUIAssociations = new Dictionary<Option, DropdownOption>();
        private const float toggleDebounceTime = 0.2f;
        private bool canToggle = true;

        [Serializable]
        public class Option
        {
            public bool isChecked;
            public string label;
            public int value;

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
            int bitField = GetBitField();
            OnChanged.Invoke(bitField);

            if (!Multiselect)
            {
                Toggle();
            }
        }

        public void SetOptions(List<Option> additionalOptions)
        {
            options = new Option[additionalOptions.Count + (IncludeGlobalOptions ? 2 : 0)];

            if (IncludeGlobalOptions)
            {
                // Resetting "All" option before appending new flags.

                options[0] = new Option("None", 0);
                options[1] = new Option("All", 1, true);
            }

            for (int i = 0; i < additionalOptions.Count; i++)
            {
                Option option = additionalOptions[i];
                string label = option.label;

                if (label.ToLower() == "none" || label.ToLower() == "all")
                {
                    return;
                }
                options[i + (IncludeGlobalOptions ? 2 : 0)] = option;

                if (IncludeGlobalOptions)
                {
                    options[1].value |= option.value;
                }
            }

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

        private void Awake()
        {
            OnBeforeSerialize();
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

            if (IncludeGlobalOptions)
            {
                if (options == null)
                {
                    options = new Option[2];
                }

                options[0] = new Option("None", 0);
                options[1] = new Option("All", 1, true);
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

        private void OnDropdownOptionSelected(Option selectedOption)
        {
            if (!Multiselect)
            {
                foreach (Option option in options)
                {
                    SetOptionChecked(option, false);
                }
            }

            SetOptionChecked(selectedOption, !selectedOption.isChecked);

            if (LabelText != null)
            {
                int checkedOptionsCount = options.Sum(option => option.isChecked ? 1 : 0);

                if (checkedOptionsCount == 1)
                {
                    Option checkedOption = options.ToList().Find(option => option.isChecked);

                    if (checkedOption != null)
                    {
                        LabelText.text = checkedOption.label;
                    }
                }
                else if (checkedOptionsCount > 1)
                {
                    LabelText.text = "Mixed";
                }
                else
                {
                    LabelText.text = "None";
                }
            }

            if (IncludeGlobalOptions)
            {
                if (selectedOption.label == "None" && selectedOption.isChecked)
                {
                    for(int i = 1; i < options.Length; i++)
                    {
                        Option iOption = options[i];
                        SetOptionChecked(iOption, false);
                    }
                }
                else if (selectedOption.label == "All" && selectedOption.isChecked)
                {
                    SetOptionChecked(options[0], false);

                    for(int i = 1; i < options.Length; i++)
                    { 
                        Option iOption = options[i];
                        SetOptionChecked(iOption, true);
                    }
                }

                SetOptionChecked(options[0], options.All(option => option.label == "None" || !option.isChecked));
                SetOptionChecked(options[1], options.All(option => (option.label == "None" && !option.isChecked) || option.label == "All" || option.isChecked));
            }

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

        public void OnBeforeSerialize()
        {
            if (_optionValues == null || _optionLabels == null || _optionIsCheckedStates == null)
            {
                return;
            }

            options = new Option[_optionValues.Length];

            for (int i = 0; i < _optionValues.Length; i++)
            {
                bool isChecked = _optionIsCheckedStates[i];
                string label = _optionLabels[i];
                int value = _optionValues[i];
                options[i] = new Option(label, value, isChecked);
            }
        }

        public void OnAfterDeserialize()
        {
            if (options == null)
            {
                return;
            }

            _optionIsCheckedStates = new bool[options.Length];
            _optionLabels = new string[options.Length];
            _optionValues = new int[options.Length];

            for (int i = 0; i < options.Length; i++)
            {
                Option option = options[i];
                _optionIsCheckedStates[i] = option.isChecked;
                _optionLabels[i] = option.label;
                _optionValues[i] = option.value;
            }
        }
    }
}
