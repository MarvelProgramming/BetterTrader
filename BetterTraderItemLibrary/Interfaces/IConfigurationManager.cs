using System.Collections.Generic;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface IConfigurationManager<T, U> where T : ITradable, new() where U : new()
    {
        U LoadTrader();
        List<T> LoadItems();
        void CreateDefaultTraderConfigurationFile();
        void CreateDefaultItemConfigurationFiles(List<T> tradableItems);
    }
}
