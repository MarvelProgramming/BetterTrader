using HarmonyLib;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{
    [HarmonyPatch(typeof(Game))]
    internal class Game_Patches
    {
        [HarmonyPatch(nameof(Game.Start)), HarmonyPrefix, HarmonyBefore("Menthus15Mods.Valheim.BetterTraderClient")]
        public static void Start()
        {
            RPC.RegisterRPCMethods();
        }
    }
}
