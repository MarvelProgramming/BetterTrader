using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ITradable
    {
        string Name { get; }
        int BasePurchasePrice { get; }
        int BaseSalesPrice { get; }
        int BaseTraderStorage { get; }
    }
}
