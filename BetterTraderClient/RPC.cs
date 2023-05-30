using Menthus15Mods.Valheim.BetterTraderLibrary;
using System.Collections.Generic;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    public static class RPC
    {
        public static void RegisterRPCMethods()
        {
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInfo), RPC_RequestTraderInfo);
        }

        [Client]
        public static void RPC_RequestTraderInfo(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                var coins = pkg.ReadInt();
                var items = new List<Item>();
                var serializer = new YamlConfigurationSerializer();

                while (pkg.GetPos() < pkg.Size())
                {
                    var item = serializer.Deserialize<Item>(pkg.ReadString());
                    items.Add(item);
                }

                EventManager.RaiseFetchedTraderInfo(coins,items);
            }
        }
    }
}
