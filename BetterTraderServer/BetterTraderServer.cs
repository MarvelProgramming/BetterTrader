using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Jotunn.Utils;
using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    [UsedImplicitly]
    public class BetterTraderServer : BaseUnityPlugin
    {
        internal static ManualLogSource LoggerInstance { get; private set; }
        internal static BTrader TraderInstance { get; private set; }
        private const string GUID = "Menthus15Mods.Valheim." + nameof(BetterTraderServer);
        private const string NAME = nameof(BetterTraderServer);
        private const string VERSION = "1.0.0";
        private ConfigurationManager configurationManager;

        #region Unity

        [UsedImplicitly]
        private void Awake()
        {
            LoggerInstance = Logger;
            PrefabManager.OnPrefabsRegistered += HandleOnPrefabsRegistered;
            EventManager.OnFinishedRecordingObjectDBItems += HandleFinishedRecordingObjectDBItems;
            EventManager.OnGameSave += HandleGameSave;
            EventManager.OnNewDay += HandleNewDay;
            SetupPatches();
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            EventManager.OnFinishedRecordingObjectDBItems -= HandleFinishedRecordingObjectDBItems;
            EventManager.OnGameSave -= HandleGameSave;
            EventManager.OnNewDay -= HandleNewDay;
        }

        [UsedImplicitly]
        private void FixedUpdate() // ToDo: Is this really the update to use?
        {
            ThreadingUtils.ExecutePendingActions();
        }

        #endregion

        private void HandleFinishedRecordingObjectDBItems(List<ITradableConfig> tradableItems, string worldSave)
        {
            string traderConfigFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(), "configs", worldSave, "trader.yml");
            string tradableItemConfigPath = Path.Combine(Path.GetDirectoryName(traderConfigFilePath) ?? throw new InvalidOperationException(), "items");
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

                if (TraderInstance.activelyPurchasableItemsList.Count == 0)
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

        private void HandleOnPrefabsRegistered()
        {
            List<ITradableConfig> tradableItems = ObjectDB.instance.GetTradableItems();

            // https://github.com/Digitalroot/Menthus123-BetterTrader/blob/c96fb1bfde80e3123dc5a6436fe294a02d11d6c5/src/BetterTraderRemake/Core/FileConfiguration.cs#LL113C11-L113C82
            string worldSaveFolderName = $"{ZNet.instance.GetWorldName()}_{ZNet.instance.GetWorldUID()}";

            EventManager.RaiseFinishedGatheringObjectDBItems(tradableItems, worldSaveFolderName);

            PrefabManager.OnPrefabsRegistered -= HandleOnPrefabsRegistered;
        }

        private void SetupPatches()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }
    }
}
