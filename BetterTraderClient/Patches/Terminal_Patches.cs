using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class Terminal_Patches
    {
#if DEBUG
        [HarmonyPatch(nameof(Terminal.IsCheatsEnabled)), HarmonyPrefix]
        private static bool IsCheatsEnabled(ref bool __result)
        {
            __result = true;

            return false;
        }
#endif
    }
}
