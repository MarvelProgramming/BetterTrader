using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal static class EventManager
    {
        public static Action<List<ITradableConfig>> OnGeneratedConfigs;
        public static Action<List<ITradableConfig>, string> OnFinishedRecordingObjectDBItems;
        public static Action OnGameSave;
        public static Action OnNewDay;

        public static void RaiseGeneratedConfigs(List<ITradableConfig> configs)
        {
            OnGeneratedConfigs?.Invoke(configs);
        }

        public static void RaiseFinishedGatheringObjectDBItems(List<ITradableConfig> tradableItems, string worldSave)
        {
            OnFinishedRecordingObjectDBItems?.Invoke(tradableItems, worldSave);
        }

        public static void RaiseGameSave()
        {
            OnGameSave?.Invoke();
        }

        public static void RaiseNewDay()
        {
            OnNewDay?.Invoke();
        }
    }
}
