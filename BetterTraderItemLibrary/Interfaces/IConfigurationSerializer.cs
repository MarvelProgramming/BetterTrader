namespace Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces
{
    public interface IConfigurationSerializer
    {
        string Serialize(object obj);
        T Deserialize<T>(string input);
    }
}
