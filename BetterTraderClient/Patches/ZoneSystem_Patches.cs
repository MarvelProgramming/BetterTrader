using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderClient.Patches
{
    [HarmonyPatch(typeof(ZoneSystem))]
    internal class ZoneSystem_Patches
    {
#if DEBUG
        [HarmonyPatch(nameof(ZoneSystem.Start)), HarmonyPostfix]
        private static void Start(ZoneSystem __instance)
        {
            /*foreach(ZoneSystem.ZoneLocation location in __instance.m_locations)
            {
                if (location.m_prefabName.Contains("Vendor"))
                {
                    ZoneSystem.instance
                    location.m_iconPlaced = true;
                    location.m_iconAlways = true;
                    break;
                }
            }*/
        }
#endif
    }
}
