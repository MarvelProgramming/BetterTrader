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
        public string Name { get; set; }
        [YamlIgnore]
        public bool RequireDiscovery { get; set; }
        public bool IsOnDiscount { get; set; }
        public bool IsActivelyPurchasable { get; set; }
        [YamlIgnore]
        public bool IsEquipped { get; set; }
        [YamlIgnore]
        public Vector2i GridPosition { get; set; }
        public int CurrentPurchasePrice { get; set; }
        public int CurrentSalesPrice { get; set; }
        public int CurrentStock { get; set; }

        // Empty .ctor required for YamlDotNet to work with this type.
        public CirculatedItem() { }

        public CirculatedItem(string name)
        {
            Name = name;
        }

        public CirculatedItem(string Name, Vector2i gridPosition) : this(Name)
        {
            GridPosition = gridPosition;
        }

        public CirculatedItem(string name, bool requireDiscovery, bool isOnDiscount, bool isActivelyPurchasable, int currentPurchasePrice, int currentSalesPrice, int currentStock)
        {
            Name = name;
            RequireDiscovery = requireDiscovery;
            IsOnDiscount = isOnDiscount;
            IsActivelyPurchasable = isActivelyPurchasable;
            CurrentPurchasePrice = currentPurchasePrice;
            CurrentSalesPrice = currentSalesPrice;
            CurrentStock = currentStock;
        }

        public CirculatedItem(string name, bool requireDiscovery, bool isOnDiscount, bool isActivelyPurchasable, bool isEquipped, Vector2i gridPosition, int currentPurchasePrice, int currentSalesPrice, int currentStock) : this(name, requireDiscovery, isOnDiscount, isActivelyPurchasable, currentPurchasePrice, currentSalesPrice, currentStock)
        {
            IsEquipped = isEquipped;
            GridPosition = gridPosition;
        }

        public void Serialize(ref ZPackage pkg)
        {
            try
            {
                pkg.Write(Name);
                pkg.Write(RequireDiscovery);
                pkg.Write(IsOnDiscount);
                pkg.Write(IsActivelyPurchasable);
                pkg.Write(IsEquipped);
                pkg.Write(GridPosition);
                pkg.Write(CurrentPurchasePrice);
                pkg.Write(CurrentSalesPrice);
                pkg.Write(CurrentStock);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to serialize {nameof(CirculatedItem)} into ZPackage!", e);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            try
            {
                string name = pkg.ReadString();
                bool requireDiscovery = pkg.ReadBool();
                bool isOnDiscount = pkg.ReadBool();
                bool isActivelyPurchasable = pkg.ReadBool();
                bool isEquipped = pkg.ReadBool();
                Vector2i gridPosition = pkg.ReadVector2i();
                int currentPurchasePrice = pkg.ReadInt();
                int currentSalesPrice = pkg.ReadInt();
                int currentStock = pkg.ReadInt();

                Name = name;
                RequireDiscovery = requireDiscovery;
                IsOnDiscount = isOnDiscount;
                IsEquipped = isEquipped;
                GridPosition = gridPosition;
                IsActivelyPurchasable = isActivelyPurchasable;
                CurrentPurchasePrice = currentPurchasePrice;
                CurrentSalesPrice = currentSalesPrice;
                CurrentStock = currentStock;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to deserialize ZPackage into {nameof(CirculatedItem)}!", e);
            }
        }
    }
}
