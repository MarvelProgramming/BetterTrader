using HarmonyLib;

namespace Menthus15Mods.Valheim.BetterTraderServer.Patches
{
    [HarmonyPatch(typeof(ZNet))]
    // ReSharper disable once InconsistentNaming
    internal class ZNet_Patches
    {
        [HarmonyPatch(nameof(ZNet.Save)), HarmonyPostfix]
        private static void Save()
        {
            if (ZNet.instance != null && (ZNet.IsSinglePlayer || ZNet.instance.IsServer()))
            {
                EventManager.RaiseGameSave();
            }
        }
    }
}
