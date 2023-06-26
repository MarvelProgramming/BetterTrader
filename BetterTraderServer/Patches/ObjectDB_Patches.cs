using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{

    [HarmonyPatch(typeof(ObjectDB)), HarmonyPriority(int.MaxValue - 999), HarmonyAfter("randyknapp.mods.epicloot", "com.jotunn.jotunn")]
    internal class ObjectDB_Patches
    {
        [HarmonyPatch(nameof(ObjectDB.Awake)), HarmonyPostfix]
        private static void Awake(ObjectDB __instance)
        {
            // Only runs if client has loaded into world.
            if (Game.instance != null && ZNet.instance != null && (ZNet.IsSinglePlayer || ZNet.instance.IsServer()))
            {
                List<ITradableConfig> tradableItems = __instance.GetTradableItems();

                // https://github.com/Digitalroot/Menthus123-BetterTrader/blob/c96fb1bfde80e3123dc5a6436fe294a02d11d6c5/src/BetterTraderRemake/Core/FileConfiguration.cs#LL113C11-L113C82
                string worldSaveFolderName = $"{ZNet.instance.GetWorldName()}_{ZNet.instance.GetWorldUID()}";

                EventManager.RaiseFinishedGatheringObjectDBItems(tradableItems, worldSaveFolderName);
            }
        }
    }
}
