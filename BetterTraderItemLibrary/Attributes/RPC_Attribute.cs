using System;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Attributes
{
    public static class RPC_Attribute
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class ServerAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public class ClientAttribute : Attribute { }
    }
}
