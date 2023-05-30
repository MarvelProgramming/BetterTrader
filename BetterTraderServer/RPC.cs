using Menthus15Mods.Valheim.BetterTraderLibrary;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal static class RPC
    {
        public static void RegisterRPCMethods()
        {
            if (ZNet.instance.IsServer())
            {
                RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInfo), RPC_RequestTraderInfo);
            }
        }

        [Server]
        public static void RPC_RequestTraderInfo(long sender, ZPackage pkg)
        {
            var traderInfoPackage = new ZPackage();
            traderInfoPackage.Write(BetterTraderServer.TraderInstance.CurrentCoins);
            var yamlSerializer = new YamlConfigurationSerializer();

            foreach (var item in BetterTraderServer.TraderInstance.CurrentItems)
            {
                var serializedItem = yamlSerializer.Serialize(item);
                traderInfoPackage.Write(serializedItem);
            }

            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestTraderInfo), new object[] { traderInfoPackage });
        }
    }
}
