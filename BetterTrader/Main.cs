using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Menthus15Mods.Valheim.BetterTrader
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(Jotunn.Main.ModGuid, "2.12.4")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class Main : BaseUnityPlugin
    {
        private Harmony _harmony;
        private BetterTraderClient.BetterTraderClient _client;
        private BetterTraderServer.BetterTraderServer _server;

        public BetterTraderClient.BetterTraderClient Client
        {
            get
            {
                return _client ??= BetterTraderClient.BetterTraderClient.Instance;
            }
        }

        public BetterTraderServer.BetterTraderServer Server
        {
            get
            {
                return _server ??= BetterTraderServer.BetterTraderServer.Instance;
            }
        }

        [UsedImplicitly] public static ConfigEntry<int> NexusId;
        public static Main Instance;

        public Main()
        {
            Instance = this;
        }

        #region Unity

        [UsedImplicitly]
        private void Awake()
        {
            try
            {
                Jotunn.Logger.LogInfo($"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
                NexusId = Config.Bind("General", "NexusID", 433, new ConfigDescription("Nexus mod ID for updates", null, new ConfigurationManagerAttributes { Browsable = false, ReadOnly = true }));

                Client?.OnAwake();
                Server?.OnAwake();
                
                _harmony = Harmony.CreateAndPatchAll(typeof(Main).Assembly, Guid);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            try
            {
                Jotunn.Logger.LogInfo($"{Namespace}.{MethodBase.GetCurrentMethod()?.DeclaringType?.Name}.{MethodBase.GetCurrentMethod()?.Name}");
                Server?.OnDestroy();
                _harmony?.UnpatchSelf();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            Server?.OnFixedUpdate();
            Client?.OnFixedUpdate();
        }

        #endregion
    }
}
