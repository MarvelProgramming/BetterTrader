using Menthus15Mods.Valheim.BetterTraderLibrary.Attributes;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public static class RPCUtils
    {
        /// <summary>
        /// A function wrapper for ZRoutedRpc.Register that attempts to reduce the probability of key collisions.
        /// </summary>
        public static void RegisterMethod(string key, Action<long, ZPackage> action)
        {
            var formattedKey = GetFormattedRPCRegistrationKey(key, action);
            var protectedAction = CreateProtectedAction(action);

            try
            {
                ZRoutedRpc.Register(formattedKey, protectedAction);
            }
            catch (ArgumentException e)
            {
                Debug.LogError($"Attempted to register {formattedKey} from {action.GetMethodInfo().DeclaringType.AssemblyQualifiedName} but it already exists in the ZRoutedRpc method dictionary!");
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// A function wrapper for ZRoutedRpc.InvokeRoutedRPC that formats the methodName such that it's
        /// the same as BetterTrader registered methods before invocation. Also handles client-server contexts.
        /// Check <see cref="ZRoutedRpc.InvokeRoutedRPC(long, string, object[])" /> for more info about the original method
        /// </summary>
        public static void InvokeClientServerRoutedRPC(long targetPeerID, string methodName, params object[] parameters)
        {
            var formattedKey = methodName;

            if (ZNet.instance.IsClientServer())
            {
                if (targetPeerID == ZRoutedRpc.instance.GetServerPeerID())
                {
                    formattedKey += "_Client";
                }
                else if (targetPeerID == ZRoutedRpc.Everybody)
                {
                    // Explicitly making another RPC to account for the uniquely keyed method that is created for client-server setups
                    InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), GetKeyWithPrefix(methodName) + "_Client", parameters);
                }
            }

            InvokeRoutedRPC(targetPeerID, formattedKey, parameters);
        }

        /// <summary>
        /// A function wrapper for ZRoutedRpc.InvokeRoutedRPC that formats the methodName such that it's
        /// the same as BetterTrader registered methods before invocation.
        /// Check <see cref="ZRoutedRpc.InvokeRoutedRPC(long, string, object[])" /> for more info about the original method
        /// </summary>
        public static void InvokeRoutedRPC(long targetPeerID, string methodName, params object[] parameters)
        {
            var pkg = new ZPackage();
            ZRpc.Serialize(parameters, ref pkg);
            pkg.SetPos(0);

            var formattedKey = GetKeyWithPrefix(methodName);
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerID, formattedKey, pkg);
        }

        private static ZRoutedRpc ZRoutedRpc => ZRoutedRpc.instance;

        private static string GetFormattedRPCRegistrationKey(string key, Action<long, ZPackage> action = null)
        {
            var formattedKey = GetKeyWithPrefix(key);
            formattedKey = GetKeyWithClientServerContextPostfix(formattedKey, action);

            return formattedKey;
        }

        /// <summary>
        /// Creates and returns a key with the unique BetterTrader prefix for RPC method registration
        /// </summary>
        /// <param name="key">Original key</param>
        private static string GetKeyWithPrefix(string key)
        {
            var formattedKey = key;

            if (!formattedKey.StartsWith(Properties.Settings.Default.RPCKeyNamePrefix))
            {
                formattedKey = Properties.Settings.Default.RPCKeyNamePrefix + formattedKey;
            }

            return formattedKey;
        }

        /// <summary>
        /// Appends a unique identifier to the input key if the executing context 
        /// is that of a client-server (i.e. a non-headless server) and returns the result.
        /// </summary>
        private static string GetKeyWithClientServerContextPostfix(string key, Action<long, ZPackage> action)
        {
            var formattedKey = key;
            var actionMethodInfo = action?.GetMethodInfo();

            if (
                ZNet.instance.IsClientServer() &&
                actionMethodInfo.GetCustomAttribute<RPC_Attribute.ClientAttribute>() != null &&
                ZRoutedRpc.instance.m_functions.ContainsKey(key.GetStableHashCode())
                )
            {
                formattedKey += "_Client";
            }

            return formattedKey;
        }

        /// <summary>
        /// A wrapper function for registered actions that determines if messages received are valid before invocation and ignores ones that aren't.
        /// This reduces the amount of redudant sanitation checks that happen across RPC methods.
        /// </summary>
        /// <param name="action">The original action to be registered</param>
        private static Action<long, ZPackage> CreateProtectedAction(Action<long, ZPackage> action)
        {
            return delegate (long sender, ZPackage pkg)
            {
                // https://github.com/Valheim-Modding/Wiki/wiki/Server-Validated-RPC-System#:~:text=public%20static%20void%20RPC_RequestServerAnnouncement,message.%0A%20%20%20%20%20%20%20%20%7D%0A%20%20%20%20%7D%0A%7D
                if (pkg != null)
                {
                    ZNetPeer peer = ZNet.instance.GetPeer(sender);

                    if (peer != null || ZNet.instance.IsClientServer() && sender == ZRoutedRpc.instance.GetServerPeerID())
                    {
                        action(sender, pkg);
                    }
                }
            };
        }
    }
}
