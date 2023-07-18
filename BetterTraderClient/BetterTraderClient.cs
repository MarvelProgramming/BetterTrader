using JetBrains.Annotations;
using Menthus15Mods.Valheim.BetterTraderClient.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    [UsedImplicitly]
    public class BetterTraderClient
    {
        public static GameObject UI_ASSET;
        public static readonly BetterTraderClient Instance;

        static BetterTraderClient()
        {
            Instance = new BetterTraderClient();
        }

        #region Unity

        public void OnAwake()
        {
            try
            {
                Jotunn.Logger.LogInfo($"{nameof(BetterTraderClient)}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
                Setup();
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

        private void Setup()
        {
            SetupAssets();
        }

        private void SetupAssets()
        {
            IAssetBundleManager assetBundleManager = new AssetBundleManager();
            UI_ASSET = assetBundleManager.LoadAssetFromBundle<GameObject>(Properties.Settings.Default.UIAssetName);
        }
    }
}
