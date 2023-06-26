using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ICirculatable : IDiscoverable
    {
        string Name { get; }
        bool IsOnDiscount { get; set; }
        bool IsActivelyPurchasable { get; set; }
        bool IsEquipped { get; set; }
        Vector2i GridPosition { get; set; }
        int CurrentPurchasePrice { get; set; }
        int CurrentSalesPrice { get; set; }
        int CurrentStock { get; set; }


        void Serialize(ref ZPackage pkg);
        void Deserialize(ref ZPackage pkg);
    }
}
