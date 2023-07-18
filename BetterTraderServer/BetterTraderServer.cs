using JetBrains.Annotations;
using Jotunn.Managers;
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
    [UsedImplicitly]
    public class BetterTraderServer
    {
        internal static BTrader TraderInstance { get; private set; }
        private ConfigurationManager configurationManager;
        public static readonly BetterTraderServer Instance;

        static BetterTraderServer()
        {
            Instance = new BetterTraderServer();
        }

        #region Unity

        public void OnAwake()
        {
            try
            {
                Jotunn.Logger.LogInfo($"{nameof(BetterTraderServer)}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
                PrefabManager.OnPrefabsRegistered += HandleOnPrefabsRegistered;
                EventManager.OnGeneratedConfigs += HandleGeneratedConfigs;
                EventManager.OnFinishedRecordingObjectDBItems += HandleFinishedRecordingObjectDBItems;
                EventManager.OnGameSave += HandleGameSave;
                EventManager.OnNewDay += HandleNewDay;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void OnDestroy()
        {
            try
            {
                Jotunn.Logger.LogInfo($"{nameof(BetterTraderServer)}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
                EventManager.OnGeneratedConfigs -= HandleGeneratedConfigs;
                EventManager.OnFinishedRecordingObjectDBItems -= HandleFinishedRecordingObjectDBItems;
                EventManager.OnGameSave -= HandleGameSave;
                EventManager.OnNewDay -= HandleNewDay;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        public void OnFixedUpdate()
        {
            try
            {
                ThreadingUtils.ExecutePendingActions();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        #endregion

        private void HandleGeneratedConfigs(List<ITradableConfig> list)
        {
            TraderInstance.ItemConfigurations = list;
            TraderInstance.UpdateAllItemAssociations();
            TraderInstance.UpdateCirculatedItems(true);
        }

        private void HandleFinishedRecordingObjectDBItems(List<ITradableConfig> tradableItems, string worldSave)
        {
            string traderConfigFilePath = Path.Combine(BepInEx.Paths.ConfigPath, "BetterTrader/configs", worldSave, "trader.yml");
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
                configurationManager.SetupFileWatchers((sender, args) =>
                {
                    Jotunn.Logger.LogInfo("Updated Trader Instance!");
                    TraderInstance = configurationManager.LoadTrader<Item>();
                    TraderInstance.UpdateAllItemAssociations();
                });
                TraderInstance.UpdateAllItemAssociations();

                if (TraderInstance.activelyPurchasableItemsList.Count == 0)
                {
                    TraderInstance.UpdateCirculatedItems(true);
                }
            }
            catch(Exception e)
            {
                Jotunn.Logger.LogError(e);
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
            string worldSaveFolderName = ZNet.instance.GetWorldSaveName();
            EventManager.RaiseFinishedGatheringObjectDBItems(tradableItems, worldSaveFolderName);
            PrefabManager.OnPrefabsRegistered -= HandleOnPrefabsRegistered;
        }
    }
}
