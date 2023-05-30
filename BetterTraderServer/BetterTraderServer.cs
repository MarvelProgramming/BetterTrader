using BepInEx;
using Menthus15Mods.Valheim.BetterTraderClient;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    [BepInProcess("valheim.exe")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterTraderServer : BaseUnityPlugin
    {
        internal static Trader TraderInstance { get; private set; }
        private const string GUID = "Menthus15Mods.Valheim." + nameof(BetterTraderServer);
        private const string NAME = nameof(BetterTraderServer);
        private const string VERSION = "1.0.0";

        private void Awake()
        {
            EventManager.OnFinishedRecordingObjectDBItems += SetupConfiguration;
            SetupPatches();
        }

        private void OnDestroy()
        {
            EventManager.OnFinishedRecordingObjectDBItems -= SetupConfiguration;
        }

        private void SetupConfiguration(List<Item> tradableItems)
        {
            var configurationSerializer = new YamlConfigurationSerializer();
            var configurationManager = new ConfigurationManager<Item, Trader>(configurationSerializer);
#if CREATE_DEFAULT_CONFIGS
            configurationManager.CreateDefaultTraderConfigurationFile();
            configurationManager.CreateDefaultItemConfigurationFiles(tradableItems);
#endif
            TraderInstance = configurationManager.LoadTrader();
            TraderInstance.CurrentItems.AddRange(configurationManager.LoadItems());
        }

        private void SetupPatches()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
