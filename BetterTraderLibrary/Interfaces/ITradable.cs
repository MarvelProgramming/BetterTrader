using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface ITradable
    {
        string Name { get; }
        int BasePurchasePrice { get; set; }
        int BaseSalesPrice { get; set; }
        int BaseTraderStorage { get; }
    }
}
