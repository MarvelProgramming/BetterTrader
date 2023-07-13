using Common;
using HarmonyLib;
using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Heightmap;
using static Terminal;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class Terminal_Patches
    {
        [HarmonyPatch(nameof(Terminal.InitTerminal)), HarmonyPostfix]
        private static void InitTerminal()
        {
            new ConsoleCommand("bt.generate.config", "Generates default item configurations. [maxStackPricingWeight] [weightPricingWeight] [teleportationPricingWeight] [prevalencePricingWeight] [usefulnessPricingWeight] [biomePricingWeight] [enemyPricingWeight] [globalWeight] [globalBasePricing]", (args) =>
            {
                float maxStackPricingWeight = args.TryParameterFloat(1);
                float weightPricingWeight = args.TryParameterFloat(2);
                float teleportationPricingWeight = args.TryParameterFloat(3);
                float prevalencePricingWeight = args.TryParameterFloat(4);
                float usefulnessPricingWeight = args.TryParameterFloat(5);
                float biomePricingWeight = args.TryParameterFloat(6);
                float enemyPricingWeight = args.TryParameterFloat(7);
                float globalWeight = args.TryParameterFloat(8);
                float globalBasePricing = args.TryParameterFloat(9);

                // Log this command's description if no parameters are provided.
                if (args.FullLine.Split(' ').Length == 1)
                {
                    args.Context.AddString(commands["bt.generate.config"].Description);
                    return true;
                }

                RPCUtils.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RPC_RequestGenerateConfigs", new object[] { maxStackPricingWeight, weightPricingWeight, teleportationPricingWeight, prevalencePricingWeight, usefulnessPricingWeight, biomePricingWeight, enemyPricingWeight, globalWeight, globalBasePricing });

                return true;
            }, false, false, true, false, false);
        }

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
