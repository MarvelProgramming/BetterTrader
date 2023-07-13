using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Utils
{
    public static class StoreGUIUtils
    {
        public static GameObject GetBtUIObject(StoreGui __instance, string uiAssetName) => __instance.transform?.Find(uiAssetName)?.gameObject;
        public static bool GetBtUIExists(StoreGui __instance, string uiAssetName) => GetBtUIObject(__instance, uiAssetName) != null;
        private static Sprite vanillaBackgroundSprite;
        public static Sprite VanillaBackgroundSprite
        {
            get
            {
                if (vanillaBackgroundSprite == null)
                {
                    vanillaBackgroundSprite = GetVanillaStoreBackground(StoreGui.instance).GetComponent<Image>().sprite;
                }

                return vanillaBackgroundSprite;
            }
        }
        private static Material vanillaBackgroundMaterial;
        public static Material VanillaBackgroundMaterial
        {
            get
            {
                if (vanillaBackgroundMaterial == null)
                {
                    vanillaBackgroundMaterial = GetVanillaStoreBackground(StoreGui.instance).GetComponent<Image>().material;
                }

                return vanillaBackgroundMaterial;
            }
        }
        private static Font vanillaHeaderFont;
        public static Font VanillaHeaderFont
        {
            get
            {
                if (vanillaHeaderFont == null)
                {
                    vanillaHeaderFont = StoreGui.instance.transform.Find("Store/topic").GetComponent<Text>().font;
                }

                return vanillaHeaderFont;
            }
        }
        private static Font vanillaSecondaryFont;
        public static Font VanillaSecondaryFont
        {
            get
            {
                if (vanillaSecondaryFont == null)
                {
                    vanillaSecondaryFont = GetVanillaStoreCoin(StoreGui.instance).Find("coins").GetComponent<Text>().font;
                }

                return vanillaSecondaryFont;
            }
        }
        private static Sprite vanillaStoreButtonImageSprite;
        public static Sprite VanillaStoreButtonImageSprite
        {
            get
            {
                if (vanillaStoreButtonImageSprite == null)
                {
                    vanillaStoreButtonImageSprite = GetVanillaStoreBuyButton(StoreGui.instance).image.sprite;
                }

                return vanillaStoreButtonImageSprite;
            }
        }
        private static Sprite vanillaStoreButtonDisabledSprite;
        public static Sprite VanillaStoreButtonDisabledSprite
        {
            get
            {
                if (vanillaStoreButtonDisabledSprite == null)
                {
                    vanillaStoreButtonDisabledSprite = GetVanillaStoreBuyButton(StoreGui.instance).spriteState.disabledSprite;
                }

                return vanillaStoreButtonDisabledSprite;
            }
        }
        private static Sprite vanillaStoreButtonHighlightedSprite;
        public static Sprite VanillaStoreButtonHighlightedSprite
        {
            get
            {
                if (vanillaStoreButtonHighlightedSprite == null)
                {
                    vanillaStoreButtonHighlightedSprite = GetVanillaStoreBuyButton(StoreGui.instance).spriteState.highlightedSprite;
                }

                return vanillaStoreButtonHighlightedSprite;
            }
        }
        private static Sprite vanillaStoreButtonPressedSprite;
        public static Sprite VanillaStoreButtonPressedSprite
        {
            get
            {
                if (vanillaStoreButtonPressedSprite == null)
                {
                    vanillaStoreButtonPressedSprite = GetVanillaStoreBuyButton(StoreGui.instance).spriteState.pressedSprite;
                }

                return vanillaStoreButtonPressedSprite;
            }
        }
        private static Sprite vanillaStoreButtonSelectedSprite;
        public static Sprite VanillaStoreButtonSelectedSprite
        {
            get
            {
                if (vanillaStoreButtonSelectedSprite == null)
                {
                    vanillaStoreButtonSelectedSprite = GetVanillaStoreBuyButton(StoreGui.instance).spriteState.selectedSprite;
                }

                return vanillaStoreButtonSelectedSprite;
            }
        }
        private static Sprite vanillaStoreCoinsSprite;
        public static Sprite VanillaStoreCoinsSprite
        {
            get
            {
                if (vanillaStoreCoinsSprite == null)
                {
                    vanillaStoreCoinsSprite = GetVanillaStoreCoin(StoreGui.instance).Find("coin icon").GetComponent<Image>().sprite;
                }

                return vanillaStoreCoinsSprite;
            }
        }
        private static Transform GetVanillaStoreBackground(StoreGui __instance) => __instance.transform.Find("Store/border (1)");
        private static Transform GetVanillaStoreCoin(StoreGui __instance) => __instance.transform.Find("Store/coins");
        private static Button GetVanillaStoreBuyButton(StoreGui __instance) => __instance.transform.Find("Store/BuyButton").GetComponent<Button>();
    }
}
