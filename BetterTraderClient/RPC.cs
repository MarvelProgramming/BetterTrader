using Menthus15Mods.Valheim.BetterTraderLibrary;
using System.Collections.Generic;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderClient
{
    public static class RPC
    {
        public static void RegisterRPCMethods()
        {
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInventory), RPC_RequestTraderInventory);
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInventory), RPC_RequestTraderInventory);
        }

        [Client]
        public static void RPC_RequestTraderInventory(long sender, ZPackage pkg)
        {
            if (sender == ZRoutedRpc.instance.GetServerPeerID() && pkg.Size() > 0)
            {
                var items = new List<Item>();
                var serializer = new YamlConfigurationSerializer();

                while (pkg.GetPos() < pkg.Size())
                {
                    var item = serializer.Deserialize<Item>(pkg.ReadString());
                    items.Add(item);
                }

                // TODO: Update this to include the trader's available coins instead of 0
                EventManager.RaiseFetchedTraderInventory(0, items);
            }
        }
    }
}
