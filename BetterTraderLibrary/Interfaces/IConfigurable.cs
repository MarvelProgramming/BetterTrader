using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface IConfigurable
    {
        bool Purchasable { get; }
        bool Sellable { get; }
        bool RequireDiscovery { get; }
        bool CanBeOnDiscount { get; }
        int DiscountScalar { get; }
        int MinSalesPrice { get; }
    }
}
