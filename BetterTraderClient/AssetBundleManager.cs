using Menthus15Mods.Valheim.BetterTraderClient.Interfaces;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    internal class AssetBundleManager : IAssetBundleManager
    {
        public AssetBundleManager() { }

        public T LoadAssetFromBundle<T>(string assetName) where T : Object
        {
            T asset;

            using (var stream = GetEmbeddedResourceStream())
            {
                var bundle = AssetBundle.LoadFromStream(stream);
                asset = bundle.LoadAsset<T>(assetName);
                bundle.Unload(false);
            }

            return asset;
        }

        private Stream GetEmbeddedResourceStream()
        {
            var embeddedResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Properties.Settings.Default.AssetBundlePath);

            return embeddedResourceStream;
        }
    }
}
