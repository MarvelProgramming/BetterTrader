using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ITradable
    {
        string Name { get; }
        ItemDrop Drop { get; }
        int PurchasePrice { get; }
        int SalesPrice { get; }
        int TraderStorage { get; }
        bool Purchasable { get; }
        bool Sellable { get; }
        bool RequireDiscovery { get; }
    }
}
