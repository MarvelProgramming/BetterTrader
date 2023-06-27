using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{
    [HarmonyPatch(typeof(Game))]
    internal class Game_Patches
    {
        [HarmonyPatch(nameof(Game.Start)), HarmonyPrefix, HarmonyBefore("Menthus15Mods.Valheim.BetterTraderClient")]
        public static void Start()
        {
            if (ZNet.instance != null && (ZNet.IsSinglePlayer || ZNet.instance.IsClientServer() || ZNet.instance.IsServer()))
            {
                RPC.RegisterRPCMethods();
            }
        }
    }
}
