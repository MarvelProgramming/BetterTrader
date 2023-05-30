using UnityEngine;
using UnityEngine.Rendering;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class ZNetExtensions
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
    }
}
