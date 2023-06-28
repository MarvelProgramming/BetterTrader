using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Utils;
using Menthus15Mods.Valheim.BetterTraderClient.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    [UsedImplicitly]
    public class BetterTraderClient : BaseUnityPlugin
    {
        public static ManualLogSource LoggerInstance;
        public static GameObject UI_ASSET;
        private const string GUID = "Menthus15Mods.Valheim." + nameof(BetterTraderClient);
        private const string NAME = nameof(BetterTraderClient);
        private const string VERSION = "1.0.0";

        #region Unity

        [UsedImplicitly]
        private void Awake()
        {
            LoggerInstance = Logger;
            Setup();
        }

        [UsedImplicitly]
        private void FixedUpdate() // ToDo: Is this really the update to use?
        {
            ThreadingUtils.ExecutePendingActions();
        }

        #endregion

        private void Setup()
        {
            SetupAssets();
            SetupPatches();
        }

        private void SetupAssets()
        {
            IAssetBundleManager assetBundleManager = new AssetBundleManager();
            UI_ASSET = assetBundleManager.LoadAssetFromBundle<GameObject>(Properties.Settings.Default.UIAssetName);
        }

        private void SetupPatches()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }
    }
}
