using HarmonyLib;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Game))]
    internal class Game_Patches
    {
        [HarmonyPatch(nameof(Game.Start)), HarmonyPrefix]
        public static void Start()
        {
            RPC.RegisterRPCMethods();
        }
    }
}
