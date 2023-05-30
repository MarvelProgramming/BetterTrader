using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderClient.Interfaces
{
    internal interface IAssetBundleManager
    {
        T LoadAssetFromBundle<T>(string assetName) where T : Object;
    }
}
