using Menthus15Mods.Valheim.BetterTraderLibrary;
using System;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal static class EventManager
    {
        public static Action<List<Item>> OnFinishedRecordingObjectDBItems;

        public static void RaiseFinishedGatheringObjectDBItems(List<Item> tradableItems)
        {
            OnFinishedRecordingObjectDBItems?.Invoke(tradableItems);
        }
    }
}
