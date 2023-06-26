using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Terminal.ConsoleCommand))]
    internal class ConsoleCommand_Patches
    {
#if DEBUG
        [HarmonyPatch(nameof(Terminal.ConsoleCommand.IsValid)), HarmonyPrefix]
        private static bool IsValid(ref bool __result)
        {
            __result = true;

            return false;
        }
#endif
    }
}
