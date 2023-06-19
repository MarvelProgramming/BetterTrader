using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.Utils
{
    public static class StoreGui_Utils
    {
        public static GameObject GetBtUIObject(StoreGui __instance) => __instance.transform?.Find(BetterTraderClient.UI_ASSET.name)?.gameObject;
        public static bool GetBtUIExists(StoreGui __instance) => GetBtUIObject(__instance) != null;
    }
}
