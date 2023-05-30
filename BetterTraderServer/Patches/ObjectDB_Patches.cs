using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using HarmonyLib;

namespace Menthus15Mods.Valheim.BetterTraderServer.patches
{

    [HarmonyPatch(typeof(ObjectDB)), HarmonyPriority(int.MaxValue - 999), HarmonyAfter("randyknapp.mods.epicloot", "com.jotunn.jotunn")]
    internal class ObjectDB_Patches
    {
        [HarmonyPatch(nameof(ObjectDB.Awake)), HarmonyPostfix]
        private static void Awake(ObjectDB __instance)
        {
            if (Game.instance != null)
            {
                var tradableItems = __instance.GetTradableItems();
                EventManager.RaiseFinishedGatheringObjectDBItems(tradableItems);
            }
        }
    }
}
