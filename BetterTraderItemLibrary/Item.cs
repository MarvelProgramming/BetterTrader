using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class Item : ITradable
    {
        public string Name { get; private set; }

        [YamlIgnore]
        public ItemDrop Drop => ObjectDBUtils.GetDropFromItemName(Name);

        public int PurchasePrice { get; set; } = 1;
        public int SalesPrice { get; set; } = 1;

        public int TraderStorage { get; set; } = 100;

        public bool Purchasable { get; private set; } = true;

        public bool Sellable { get; private set; } = true;
        public bool RequireDiscovery { get; private set; } = false;

        public Item() { }

        public Item(string name)
        {
            Name = name;
        }

        public Item(string name, int purchasePrice, int salesPrice, int traderStorage, bool purchasable, bool sellable, bool requireDiscovery)
        {
            Name = name;
            PurchasePrice = purchasePrice;
            SalesPrice = salesPrice;
            TraderStorage = traderStorage;
            Purchasable = purchasable;
            Sellable = sellable;
            RequireDiscovery = requireDiscovery;
        }
    }
}
