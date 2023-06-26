using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class Item : ITradableConfig
    {
        public string Name { get; private set; }

        [YamlIgnore]
        public ItemDrop Drop => ObjectDBUtils.GetDropFromItemName(Name);

        public int BasePurchasePrice { get; private set; } = 1;
        public int BaseSalesPrice { get; private set; } = 1;

        public int BaseTraderStorage { get; private set; } = 100;

        public bool Purchasable { get; private set; } = true;

        public bool Sellable { get; private set; } = true;
        public bool RequireDiscovery { get; set; } = false;

        public bool CanBeOnDiscount { get; private set; } = false;

        public int DiscountScalar { get; private set; } = 1;

        public int MinSalesPrice { get; private set; } = 1;

        // Empty .ctor required for YamlDotNet to work with this type
        public Item() { }

        public Item(string name)
        {
            Name = name;
        }

        public Item(string name, int purchasePrice, int salesPrice, int traderStorage, bool purchasable, bool sellable, bool requireDiscovery, bool canBeOnDiscount, int discountScalar, int minSalesPrice)
        {
            Name = name;
            BasePurchasePrice = purchasePrice;
            BaseSalesPrice = salesPrice;
            BaseTraderStorage = traderStorage;
            Purchasable = purchasable;
            Sellable = sellable;
            RequireDiscovery = requireDiscovery;
            CanBeOnDiscount = canBeOnDiscount;
            DiscountScalar = discountScalar;
            MinSalesPrice = minSalesPrice;
        }
    }
}
