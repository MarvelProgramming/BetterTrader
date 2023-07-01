using Menthus15Mods.Valheim.BetterTraderClient.Attributes;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class SortingManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        public SortingMode CurrentSortingMode { get; private set; } = SortingMode.Alphabetical;
        public SortingState CurrentSortingState { get; private set; } = SortingState.Off;
#pragma warning disable IDE0044 // Add readonly modifier
        [SerializeField]
        private Option[] options;
        [SerializeField, HideInInspector]
        private SortingMode[] _optionSortingModes;
        [SerializeField, HideInInspector]
        private SortingOption[] _optionUIS;
        [SerializeField]
        private string[] sortingStateDecorations;
        [SerializeField]
        private UnityEvent onChanged;
#pragma warning restore IDE0044 // Add readonly modifier
        public enum SortingMode
        {
            Alphabetical,
            Stack,
            Value
        }
        public enum SortingState
        {
            Off,
            Ascending,
            Descending,
        }
        [Serializable]
        public class Option
        {
            [field: SerializeField, Enum(typeof(SortingMode))]
            public SortingMode SortingMode { get; private set; }
            [field: SerializeField]
            public SortingOption UI { get; private set; }

            public Option() { }

            public Option(SortingMode sortingMode, SortingOption ui)
            {
                SortingMode = sortingMode;
                UI = ui;
            }
        }

        public void SetItemSortingMode(SortingMode newSortingMode)
        {
            if (CurrentSortingMode == newSortingMode)
            {
                CurrentSortingState = (SortingState)(((int)CurrentSortingState + 1) % Enum.GetNames(typeof(SortingState)).Length);
            }
            else
            {
                CurrentSortingMode = newSortingMode;
                CurrentSortingState = SortingState.Ascending;
            }

            UpdateUI();
            onChanged.Invoke();
        }

        public void SetItemSortingState(SortingState newSortingState, bool notify = true)
        {
            CurrentSortingState = newSortingState;
            UpdateUI();

            if (notify)
            {
                onChanged.Invoke();
            }
        }

        private void Awake()
        {
            OnBeforeSerialize();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void OnDisable()
        {
            CurrentSortingState = SortingState.Off;
        }

        private void InitializeUI()
        {
            foreach (Option option in options)
            {
                option.UI.OnClick.AddListener(() => SetItemSortingMode(option.SortingMode));
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (Option option in options)
            {
                option.UI.StateDecoration.text = option.SortingMode == CurrentSortingMode ? sortingStateDecorations[(int)CurrentSortingState] : string.Empty;
            }
        }

        // Need to implement custom serialization for array of Options, since Unity won't do it by itself.
        public void OnBeforeSerialize()
        {
            if (_optionSortingModes == null || _optionUIS == null)
            {
                return;
            }

            options = new Option[_optionSortingModes.Length];

            for (int i = 0; i < _optionSortingModes.Length; i++)
            {
                options[i] = new Option(_optionSortingModes[i], _optionUIS[i]);
            }
        }

        public void OnAfterDeserialize()
        {
            if (options == null)
            {
                return;
            }

            _optionSortingModes = new SortingMode[options.Length];
            _optionUIS = new SortingOption[options.Length];

            for (int i = 0; i < options.Length; i++)
            {
                _optionSortingModes[i] = options[i].SortingMode;
                _optionUIS[i] = options[i].UI;
            }
        }
    }
}
