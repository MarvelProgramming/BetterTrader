using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ICirculatable
    {
        string Name { get; }
        bool IsOnDiscount { get; }
        int CurrentPurchasePrice { get; }
        int CurrentSalesPrice { get; }
        int CurrentStock { get; }
    }
}
