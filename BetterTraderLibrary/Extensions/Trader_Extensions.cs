using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class Trader_Extensions
    {
        public static bool IsHaldor(this Trader trader)
        {
            return trader.m_name == "$npc_haldor";
        }
    }
}
