using BepInEx;
using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderClient.Interfaces;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    [BepInProcess("valheim.exe")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class BetterTraderClient : BaseUnityPlugin
    {
        private const string GUID = "Menthus15Mods.Valheim." + nameof(BetterTraderClient);
        private const string NAME = nameof(BetterTraderClient);
        private const string VERSION = "1.0.0";
        public static GameObject UI_ASSET;

        private void Awake()
        {
            Setup();
        }

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
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
