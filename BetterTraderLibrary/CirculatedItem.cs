using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class CirculatedItem : ICirculatedItem
    {
        [YamlIgnore]
        public ItemDrop Drop => ObjectDBUtils.GetDropFromItemName(Name);
        public string Name { get; private set; }
        public bool IsOnDiscount { get; private set; }
        public int CurrentPurchasePrice { get; private set; }
        public int CurrentSalesPrice { get; private set; }
        public int CurrentStock { get; private set; }

        // Empty .ctor required for YamlDotNet to work with this type.
        public CirculatedItem() { }

        public CirculatedItem(string name)
        {
            Name = name;
        }

        public CirculatedItem(string name, bool isOnDiscount, int currentPurchasePrice, int currentSalesPrice, int currentStock)
        {
            Name = name;
            IsOnDiscount = isOnDiscount;
            CurrentPurchasePrice = currentPurchasePrice;
            CurrentSalesPrice = currentSalesPrice;
            CurrentStock = currentStock;
        }
    }
}
