using System.IO;
using System.Reflection;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class ConfigurationUtils
    {
        public static string ConfigPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "config", ZNet.instance.GetWorldName());
    }
}
