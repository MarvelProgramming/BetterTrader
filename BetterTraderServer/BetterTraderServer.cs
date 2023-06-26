using BepInEx;
using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using BepInEx.Logging;
using System.IO;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterTraderServer : BaseUnityPlugin
    {
        internal static ManualLogSource LoggerInstance { get; private set; }
        internal static BetterTraderLibrary.BTrader TraderInstance { get; private set; }
        private const string GUID = "Menthus15Mods.Valheim." + nameof(BetterTraderServer);
        private const string NAME = nameof(BetterTraderServer);
        private const string VERSION = "1.0.0";
        private ConfigurationManager configurationManager;

        private void Awake()
        {
            LoggerInstance = Logger;
            EventManager.OnFinishedRecordingObjectDBItems += HandleFinishedRecordingObjectDBItems;
            EventManager.OnGameSave += HandleGameSave;
            EventManager.OnNewDay += HandleNewDay;
            SetupPatches();
        }

        private void OnDestroy()
        {
            EventManager.OnFinishedRecordingObjectDBItems -= HandleFinishedRecordingObjectDBItems;
            EventManager.OnGameSave -= HandleGameSave;
            EventManager.OnNewDay -= HandleNewDay;
        }

        private void FixedUpdate()
        {
            ThreadingUtils.ExecutePendingActions();
        }

        private void HandleFinishedRecordingObjectDBItems(List<ITradableConfig> tradableItems, string worldSave)
        {
            string traderConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "configs", worldSave, "trader.yml");
            string tradableItemConfigPath = Path.Combine(Path.GetDirectoryName(traderConfigFilePath), "items");
            Dictionary<string, ISerializer> serializersByFileExtension = new Dictionary<string, ISerializer>()
            {
                { ".yml", new YamlSerializer() },
                { ".yaml", new YamlSerializer() }
            };

            configurationManager = new ConfigurationManager(traderConfigFilePath, tradableItemConfigPath, serializersByFileExtension);

            try
            {
                configurationManager.GenerateDefaultTraderConfig();
                configurationManager.GenerateDefaultItemConfigs<Item>(tradableItems);
                TraderInstance = configurationManager.LoadTrader<Item>();
                TraderInstance.UpdateAllItemAssociations();

                if (TraderInstance.activelyPurchasableItems.Count == 0)
                {
                    TraderInstance.UpdateCirculatedItems(skipRefreshIntervalCheck: true);
                }
            }
            catch(Exception e)
            {
                LoggerInstance.LogError(e);
            }
        }

        private void HandleGameSave()
        {
            configurationManager.Save(TraderInstance);
        }

        private void HandleNewDay()
        {
            TraderInstance.IncreaseDaysSinceLastInventoryRefresh();
            TraderInstance.UpdateCirculatedItems();
        }

        private void SetupPatches()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
