using UnityEngine;
using UnityEngine.Rendering;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class ZNet_Extensions
    {
        /// <summary>
        /// Checks whether the zNet instance is a non-headless server.
        /// </summary>
        public static bool IsClientServer(this ZNet zNet)
        {
            // https://github.com/Digitalroot-Valheim/Digitalroot.Valheim.Common.Utils/blob/main/src/Digitalroot.Valheim.Common.Utils/Utils.cs#L55
            var isHeadless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

            return zNet.IsServer() && !isHeadless;
        }

        public static string GetWorldSaveName(this ZNet zNet)
        {
            // https://github.com/Digitalroot/Menthus123-BetterTrader/blob/c96fb1bfde80e3123dc5a6436fe294a02d11d6c5/src/BetterTraderRemake/Core/FileConfiguration.cs#LL113C11-L113C82
            return $"{zNet.GetWorldName()}_{zNet.GetWorldUID()}";
        }
    }
}
