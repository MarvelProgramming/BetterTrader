using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal static class EventManager
    {
        public static Action<List<ITradableConfig>, string> OnFinishedRecordingObjectDBItems;

        public static void RaiseFinishedGatheringObjectDBItems(List<ITradableConfig> tradableItems, string worldSave)
        {
            OnFinishedRecordingObjectDBItems?.Invoke(tradableItems, worldSave);
        }
    }
}
