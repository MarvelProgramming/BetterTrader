using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderClient.MonoBehaviours
{
    public class TradeMenuMock : MonoBehaviour
    {
        [SerializeField]
        private MockType mockType = MockType.Background;
        private TMP_Text text;

        public enum MockType
        {
            Background = 1,
            Button = 2,
            HeadingText = 4,
            SecondaryText = 8,
            CoinsIcon = 16,
            LitPanelOnly = 32
        }

        private void Awake()
        {
            // Was originally using the "Flags" attribute for "MockType", so a regular switch statement wouldn't work. Don't
            // currently need flags so it's been disabled, but I'll keep it this way in case that changes in the future.
            if ((mockType & MockType.Background) != 0) 
            {
                if (TryGetComponent(out Image background))
                {
                    background.sprite = StoreGUIUtils.VanillaBackgroundSprite;
                    background.material = StoreGUIUtils.VanillaBackgroundMaterial;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.Background)} for object {name}.");
                }
            }

            if ((mockType & MockType.Button) != 0)
            {
                if (TryGetComponent(out Button button))
                {
                    button.image.sprite = StoreGUIUtils.VanillaStoreButtonImageSprite;

                    SpriteState ss = new SpriteState();
                    ss.disabledSprite = StoreGUIUtils.VanillaStoreButtonDisabledSprite;
                    ss.highlightedSprite = StoreGUIUtils.VanillaStoreButtonHighlightedSprite;
                    ss.pressedSprite = StoreGUIUtils.VanillaStoreButtonPressedSprite;
                    ss.selectedSprite = StoreGUIUtils.VanillaStoreButtonSelectedSprite;
                    button.spriteState = ss;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.Button)} for object {name}.");
                }
            }

            if ((mockType & MockType.HeadingText) != 0)
            {
                if (TryGetComponent(out TMP_Text headingText))
                {
                    TMP_FontAsset headingFont = TMP_FontAsset.CreateFontAsset(StoreGUIUtils.VanillaHeaderFont);
                    headingText.font = headingFont;
                    text = headingText;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.HeadingText)} for object {name}.");
                }
            }

            if ((mockType & MockType.SecondaryText) != 0)
            {
                if (TryGetComponent(out TMP_Text secondaryText))
                {
                    TMP_FontAsset secondaryFont = TMP_FontAsset.CreateFontAsset(StoreGUIUtils.VanillaSecondaryFont);
                    secondaryText.font = secondaryFont;
                    text = secondaryText;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.SecondaryText)} for object {name}.");
                }
            }

            if ((mockType & MockType.CoinsIcon) != 0)
            {
                if (TryGetComponent(out Image coinsImage))
                {
                    coinsImage.sprite = StoreGUIUtils.VanillaStoreCoinsSprite;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.CoinsIcon)} for object {name}.");
                }
            }

            if ((mockType & MockType.LitPanelOnly) != 0)
            {
                if (TryGetComponent(out Image litPanelImage))
                {
                    litPanelImage.material = StoreGUIUtils.VanillaBackgroundMaterial;
                }
                else if (TryGetComponent(out TMP_Text litPanelText))
                {
                    text = litPanelText;
                }
                else
                {
                    BetterTraderClient.LoggerInstance.LogWarning($"Failed to mock {nameof(MockType.LitPanelOnly)} for object {name}.");
                }
            }

            if (text != null)
            {
                InvokeRepeating(nameof(UpdateColor), 0, 1f);
            }
        }

        private void UpdateColor()
        {
            Color.RGBToHSV(Shader.GetGlobalColor(EnvMan.s_sunFogColor), out float hue, out float saturation, out float val);
            text.color = Color.HSVToRGB(hue, saturation, Mathf.Max(val * 1.5f, 0.75f));
        }
    }
}
