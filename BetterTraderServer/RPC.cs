using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Collections.Generic;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal static class RPC
    {
        public static void RegisterRPCMethods()
        {
            if (ZNet.instance.IsServer())
            {
                RPCUtils.RegisterMethod(nameof(RPC_RequestTraderCoins), RPC_RequestTraderCoins);
                RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInventory), RPC_RequestTraderInventory);
            }
        }

        [Server]
        public static void RPC_RequestTraderCoins(long sender, ZPackage _)
        {
            var package = new ZPackage();
            package.Write(BetterTraderServer.TraderInstance.CurrentCoins);
            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestTraderCoins), package);
        }

        [Server]
        public static void RPC_RequestTraderInventory(long sender, ZPackage _)
        {
            var package = new ZPackage();
            var yamlSerializer = new YamlSerializer();
            List<ICirculatedItem> items = BetterTraderServer.TraderInstance.GetItemsInCirculation();

            foreach (var item in items)
            {
                var serializedItem = yamlSerializer.Serialize(item);
                package.Write(serializedItem);
            }

            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestTraderInventory), new object[] { package });
        }
    }
}
