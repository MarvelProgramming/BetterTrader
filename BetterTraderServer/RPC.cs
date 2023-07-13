using Menthus15Mods.Valheim.BetterTraderLibrary;
using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using static Heightmap;
using static Menthus15Mods.Valheim.BetterTraderLibrary.Attributes.RPC_Attribute;

namespace Menthus15Mods.Valheim.BetterTraderServer
{
    internal static class RPC
    {
        private static ConcurrentDictionary<long, Thread> inventoryRequests = new ConcurrentDictionary<long, Thread>();
        private static readonly Dictionary<Heightmap.Biome, int> biomePricingWeights = new Dictionary<Heightmap.Biome, int>()
                    {
                        { Heightmap.Biome.Meadows, 1 },
                        { Heightmap.Biome.BlackForest, 2 },
                        { Heightmap.Biome.Ocean, 3 },
                        { Heightmap.Biome.Swamp, 4 },
                        { Heightmap.Biome.Mountain, 5 },
                        { Heightmap.Biome.Plains, 6 },
                        { Heightmap.Biome.Mistlands, 7 },
                        { Heightmap.Biome.AshLands, 7 },
                        { Heightmap.Biome.DeepNorth, 7 },
                    };
        private static readonly Dictionary<Character.Faction, Heightmap.Biome> factionToBiomeAssociations = new Dictionary<Character.Faction, Biome>()
                    {
                        { Character.Faction.AnimalsVeg, Biome.Meadows },
                        { Character.Faction.ForestMonsters, Biome.BlackForest },
                        { Character.Faction.SeaMonsters, Biome.Ocean },
                        { Character.Faction.MountainMonsters, Biome.Mountain },
                        { Character.Faction.PlainsMonsters, Biome.Plains },
                        { Character.Faction.MistlandsMonsters, Biome.Mistlands },
                        { Character.Faction.Undead, Biome.BlackForest },
                        { Character.Faction.Dverger, Biome.Swamp },
                        { Character.Faction.Demon, Biome.Mistlands },
                        { Character.Faction.Boss, Biome.DeepNorth },
                    };
        private static readonly Dictionary<Room.Theme, Heightmap.Biome> roomThemeToBiomeAssociations = new Dictionary<Room.Theme, Biome>
        {
            { Room.Theme.MeadowsFarm, Biome.Meadows },
            { Room.Theme.MeadowsVillage, Biome.Meadows },
            { Room.Theme.Crypt, Biome.BlackForest },
            { Room.Theme.DvergerBoss, Biome.BlackForest },
            { Room.Theme.ForestCrypt, Biome.BlackForest },
            { Room.Theme.DvergerTown, Biome.BlackForest },
            { Room.Theme.Cave, Biome.Mountain },
            { Room.Theme.GoblinCamp, Biome.Plains },
        };
        private delegate void PriceCalculator(ref float prevalence, ref float biome, ref float enemy);
        private delegate void NodeBuildPieceSetter(TradableConfigNode node);

        private class TradableConfigNode
        {
            public readonly List<TradableConfigNode> nodes = new List<TradableConfigNode>();
            public Recipe recipe;
            public float maxStack;
            public float weight;
            public float prevalence;
            public float biome;
            public float enemy;
            public PieceTable pieceTable;
            public bool isMadeFromSmelter;
            public float usefulness;
            public float teleportation;
            public readonly ITradableConfig config;

            public TradableConfigNode(ITradableConfig config)
            {
                this.config = config;
            }
        }

        public static void RegisterRPCMethods()
        {
            RPCUtils.RegisterMethod(nameof(RPC_RequestRepairItems), RPC_RequestRepairItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestTraderInfo), RPC_RequestTraderInfo);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailablePurchaseItems), RPC_RequestAvailablePurchaseItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestAvailableSellItems), RPC_RequestAvailableSellItems);
            RPCUtils.RegisterMethod(nameof(RPC_RequestPurchaseItem), RPC_RequestPurchaseItem);
            RPCUtils.RegisterMethod(nameof(RPC_RequestSellItem), RPC_RequestSellItem);
            RPCUtils.RegisterMethod(nameof(RPC_RequestGenerateConfigs), RPC_RequestGenerateConfigs);
        }

        [Server]
        public static void RPC_RequestRepairItems(long sender, ZPackage pkg)
        {
            int quantity = pkg.ReadInt();

            if (BetterTraderServer.TraderInstance.CanRepairItems)
            {
                BetterTraderServer.TraderInstance.RepairItems(quantity);
            }

            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestRepairItems), BetterTraderServer.TraderInstance.CanRepairItems, quantity, BetterTraderServer.TraderInstance.PerItemRepairCost);
        }

        [Server]
        public static void RPC_RequestTraderInfo(long sender, ZPackage pkg)
        {
            var infoPackage = new ZPackage();
            BetterTraderServer.TraderInstance.GetInfo(ref infoPackage);
            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestTraderInfo), infoPackage);
        }

        [Server]
        public static void RPC_RequestAvailablePurchaseItems(long sender, ZPackage _)
        {
            if (!inventoryRequests.ContainsKey(sender))
            {
                inventoryRequests.TryAdd(sender, null);
            }

            int requestID = DateTime.Now.ToString().GetStableHashCode();
            Thread senderThread = inventoryRequests[sender];

            if (senderThread == null || !senderThread.IsAlive)
            {
                var requestThread = new Thread(() =>
                {
                    Stack<ICirculatedItem> items = new Stack<ICirculatedItem>(BetterTraderServer.TraderInstance.GetItemsClientCanPurchase());
                    ISocket senderSocket = ZNet.instance.GetPeer(sender)?.m_socket;
                    int totalItemsToSend = items.Count;

                    while (items.Count > 0)
                    {
                        // TODO: Accessing "send queue size" in this way is not thread safe. Find workaround or alternative.
                        while (senderSocket != null && senderSocket.GetSendQueueSize() >= 20000)
                        {
                            Thread.Sleep(0);
                        }

                        var segmentedPackage = new ZPackage();
                        segmentedPackage.Write(requestID);
                        int packageSize = Math.Min(10, items.Count);
                        segmentedPackage.Write(totalItemsToSend);
                        segmentedPackage.Write(packageSize);

                        for (int i = 0; i < packageSize; i++)
                        {
                            ICirculatedItem item = items.Pop();
                            item.Serialize(ref segmentedPackage);
                        }

                        ThreadingUtils.ExecuteOnMainThread(() => RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestAvailablePurchaseItems), new object[] { segmentedPackage }));
                        Thread.Sleep(0);
                    }
                });

                requestThread.IsBackground = true;
                inventoryRequests[sender] = requestThread;
                requestThread.Start();
            }
        }

        [Server]
        public static void RPC_RequestAvailableSellItems(long sender, ZPackage pkg)
        {
            ZPackage playerInventoryPkg = pkg.ReadPackage();
            int playerInventoryItemCount = playerInventoryPkg.ReadInt();
            var responsePackage = new ZPackage();
            List<ICirculatedItem> items = new List<ICirculatedItem>();

            for (int i = 0; i < playerInventoryItemCount; i++)
            {
                var item = new CirculatedItem();
                item.Deserialize(ref playerInventoryPkg);
                items.Add(item);
            }

            BetterTraderServer.TraderInstance.GetItemsClientCanSell(items, ref responsePackage);
            RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestAvailableSellItems), responsePackage);
        }

        [Server]
        public static void RPC_RequestPurchaseItem(long sender, ZPackage pkg)
        {
            string itemToPurchase = pkg.ReadString();
            int quantity = pkg.ReadInt();

            if (BetterTraderServer.TraderInstance.CanSell(itemToPurchase, quantity))
            {
                int purchasePrice = BetterTraderServer.TraderInstance.hashItemAssociations[itemToPurchase.GetStableHashCode()].Item1.CurrentPurchasePrice;
                BetterTraderServer.TraderInstance.PurchaseItem(itemToPurchase, quantity);
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestPurchaseItem), true, itemToPurchase, quantity, purchasePrice * quantity);
            }
            else
            {
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestPurchaseItem), false);
            }
        }

        [Server]
        public static void RPC_RequestSellItem(long sender, ZPackage pkg)
        {
            ZPackage requestPackage = pkg.ReadPackage();
            int quantity = requestPackage.ReadInt();
            ICirculatedItem inventoryItem = new CirculatedItem();
            inventoryItem.Deserialize(ref requestPackage);

            if (BetterTraderServer.TraderInstance.CanBeSold(quantity, inventoryItem))
            {
                var responsePackage = new ZPackage();
                ICirculatedItem equivalentTraderItem = BetterTraderServer.TraderInstance.GetItem(inventoryItem.Name);
                inventoryItem.CurrentSalesPrice = equivalentTraderItem.CurrentSalesPrice;
                responsePackage.Write(quantity);
                inventoryItem.Serialize(ref responsePackage);
                BetterTraderServer.TraderInstance.SellItem(inventoryItem.Name, quantity);
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestSellItem), true, responsePackage);
            }
            else
            {
                RPCUtils.InvokeClientServerRoutedRPC(sender, nameof(RPC_RequestSellItem), false);
            }
        }

        [Server]
        public static void RPC_RequestGenerateConfigs(long sender, ZPackage pkg)
        {
            if (sender != ZRoutedRpc.instance.GetServerPeerID() || pkg.Size() == 0)
            {
                return;
            }

            float maxStackPricingWeight = pkg.ReadSingle();
            float weightPricingWeight = pkg.ReadSingle();
            float teleportationPricingWeight = pkg.ReadSingle();
            float prevalencePricingWeight = pkg.ReadSingle();
            float usefulnessPricingWeight = pkg.ReadSingle();
            float biomePricingWeight = pkg.ReadSingle();
            float enemyPricingWeight = pkg.ReadSingle();
            float globalWeight = pkg.ReadSingle();
            float globalBasePricing = pkg.ReadSingle();
            List<Recipe> recipes = ObjectDB.instance.m_recipes;
            List<ITradableConfig> configs = ObjectDB.instance.GetTradableItems();
            List<TradableConfigNode> configNodes = new List<TradableConfigNode>();
            Dictionary<ITradableConfig, TradableConfigNode> configToNodeAssociations = new Dictionary<ITradableConfig, TradableConfigNode>();
            Dictionary<string, PriceCalculator> itemToWeightCalculationAssociations = new Dictionary<string, PriceCalculator>();
            Dictionary<string, NodeBuildPieceSetter> itemToNodeBuildPieceSetterAssociations = new Dictionary<string, NodeBuildPieceSetter>();
            Dictionary<string, Recipe> itemtoRecipeAssociations = new Dictionary<string, Recipe>();
            Dictionary<string, ITradableConfig> itemNameToConfigAssociations = new Dictionary<string, ITradableConfig>();
            HashSet<TradableConfigNode> completedConfigNodes = new HashSet<TradableConfigNode>();

            foreach (ZoneSystem.ZoneLocation location in ZoneSystem.instance.m_locations)
            {
                /*foreach (RandomSpawn randomSpawn in location.m_randomSpawns)
                {
                    if (randomSpawn.TryGetComponent(out Pickable pickable))
                    {
                        if (configNode.config.Drop.name == pickable.m_itemPrefab.name)
                        {
                            foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                            {
                                if ((location.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                {
                                    biome = biomeVal;
                                }
                            }

                            prevalence += (randomSpawn.m_chanceToSpawn / 100f) * 0.5f;
                        }
                    }
                }*/

                if (location.m_location != null)
                {
                    List<DropTable> dropTables = new List<DropTable>();

                    foreach (Transform locationChildObject in location.m_location.gameObject.GetAllChildTransforms())
                    {
                        if (locationChildObject.TryGetComponent(out Pickable pickable))
                        {
                            if (pickable.m_amount == 0)
                            {
                                continue;
                            }

                            SetItemWeightCalculationAssociation(pickable.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                            {
                                foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                {
                                    if ((location.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                    {
                                        biome = biomeVal;
                                    }
                                }

                                prevalence += 1f;
                            });
                        }
                        else if (locationChildObject.TryGetComponent(out PickableItem pickableItem))
                        {
                            foreach (PickableItem.RandomItem randomPickableItem in pickableItem.m_randomItemPrefabs)
                            {
                                SetItemWeightCalculationAssociation(randomPickableItem.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                {
                                    foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                    {
                                        if ((location.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                        {
                                            biome = biomeVal;
                                        }
                                    }

                                    prevalence += 1f;
                                });
                            }
                        }
                        else if (locationChildObject.TryGetComponent(out Container container))
                        {
                            dropTables.Add(container.m_defaultItems);
                        }
                        else if (locationChildObject.TryGetComponent(out DropOnDestroyed dropOnDestroyed))
                        {
                            dropTables.Add(dropOnDestroyed.m_dropWhenDestroyed);
                        }
                        else if (locationChildObject.TryGetComponent(out IDestructible destructible))
                        {
                            if (destructible is Destructible concreteDestructible && concreteDestructible.enabled)
                            {
                                if (concreteDestructible.m_spawnWhenDestroyed != null && concreteDestructible.m_spawnWhenDestroyed.TryGetComponent(out MineRock5 mineRock5))
                                {
                                    dropTables.Add(mineRock5.m_dropItems);
                                }
                            }
                            else if (destructible is TreeBase treeBase && treeBase.enabled)
                            {
                                dropTables.Add(treeBase.m_dropWhenDestroyed);

                                if (treeBase.m_logPrefab != null && treeBase.m_logPrefab.TryGetComponent(out TreeLog treeLog))
                                {
                                    if (treeLog.m_subLogPrefab != null && treeLog.m_subLogPrefab.TryGetComponent(out TreeLog subTreeLog))
                                    {
                                        dropTables.Add(subTreeLog.m_dropWhenDestroyed);
                                    }
                                }
                            }
                            else if (destructible is MineRock mineRock && mineRock.enabled)
                            {
                                dropTables.Add(mineRock.m_dropItems);
                            }
                            else if (destructible is MineRock5 mineRock5 && mineRock5.enabled)
                            {
                                dropTables.Add(mineRock5.m_dropItems);
                            }
                        }
                        else if (locationChildObject.TryGetComponent(out CreatureSpawner creatureSpawner))
                        {
                            if (creatureSpawner.m_creaturePrefab != null && creatureSpawner.m_creaturePrefab.TryGetComponent(out CharacterDrop characterDrop))
                            {
                                foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
                                {
                                    if (creatureSpawner.m_creaturePrefab.TryGetComponent(out Humanoid humanoid))
                                    {
                                        SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                        {
                                            if (enemy == 0)
                                            {
                                                enemy = GetEnemyValue(humanoid);
                                            }

                                            if (factionToBiomeAssociations.TryGetValue(humanoid.m_faction, out Heightmap.Biome spawnHumanoidBiome))
                                            {
                                                if (biomePricingWeights[spawnHumanoidBiome] < biome)
                                                {
                                                    biome = biomePricingWeights[spawnHumanoidBiome];
                                                }
                                            }
                                        });
                                    }

                                    SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                    {
                                        prevalence += (drop.m_chance / 1f);
                                    });
                                }
                            }
                        }
                        else if (locationChildObject.TryGetComponent(out OfferingBowl offeringBowl))
                        {
                            if (offeringBowl.m_bossPrefab != null && offeringBowl.m_bossPrefab.TryGetComponent(out CharacterDrop characterDrop))
                            {
                                foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
                                {
                                    if (offeringBowl.m_bossPrefab.TryGetComponent(out Humanoid humanoid))
                                    {
                                        SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                        {
                                            if (enemy == 0)
                                            {
                                                enemy = GetEnemyValue(humanoid);
                                            }

                                            if (factionToBiomeAssociations.TryGetValue(humanoid.m_faction, out Heightmap.Biome spawnHumanoidBiome))
                                            {
                                                if (biomePricingWeights[spawnHumanoidBiome] < biome)
                                                {
                                                    biome = biomePricingWeights[spawnHumanoidBiome];
                                                }
                                            }

                                            prevalence += (drop.m_chance / 1f) * 0.5f;
                                        });
                                    }
                                }
                            }
                        }
                    }

                    foreach (DropTable dropTable in dropTables)
                    {
                        if (dropTable.IsEmpty())
                        {
                            continue;
                        }

                        foreach (DropTable.DropData drop in dropTable.m_drops)
                        {
                            if (dropTable.m_dropChance == 0)
                            {
                                continue;
                            }

                            SetItemWeightCalculationAssociation(drop.m_item.name, (ref float prevalence, ref float biome, ref float enemy) =>
                            {
                                foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                {
                                    if ((location.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                    {
                                        biome = biomeVal;
                                    }
                                }

                                prevalence += (dropTable.m_dropChance / 1f);
                            });
                        }
                    }
                }
            }

            foreach (DungeonDB.RoomData roomData in DungeonDB.instance.m_rooms)
            {
                if (roomData.m_room == null)
                {
                    continue;
                }

                foreach (Transform roomObject in roomData.m_room.gameObject.GetAllChildTransforms())
                {
                    RandomSpawn randomSpawn = roomObject.GetComponent<RandomSpawn>();

                    if (roomObject.TryGetComponent(out CreatureSpawner creatureSpawner))
                    {
                        if (creatureSpawner.m_creaturePrefab.TryGetComponent(out CharacterDrop characterDrop))
                        {
                            foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
                            {
                                if (creatureSpawner.m_creaturePrefab.TryGetComponent(out Humanoid humanoid))
                                {
                                    SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                    {
                                        if (enemy == 0)
                                        {
                                            enemy = GetEnemyValue(humanoid);
                                        }

                                        if (factionToBiomeAssociations.TryGetValue(humanoid.m_faction, out Heightmap.Biome spawnHumanoidBiome))
                                        {
                                            if (biomePricingWeights[spawnHumanoidBiome] < biome)
                                            {
                                                biome = biomePricingWeights[spawnHumanoidBiome];
                                            }
                                        }
                                    });
                                }
                                else
                                {
                                    SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                    {
                                        foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                        {
                                            if (roomThemeToBiomeAssociations.TryGetValue(roomData.m_room.m_theme, out Heightmap.Biome roomBiome) && (roomBiome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                            {
                                                biome = biomeVal;
                                            }
                                        }
                                    });
                                }

                                SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                {
                                    prevalence += (drop.m_chance / 1f) * (randomSpawn != null && randomSpawn.m_chanceToSpawn > 0f ? randomSpawn.m_chanceToSpawn / 100f : 1);
                                });
                            }
                        }
                    }
                    else if (roomObject.TryGetComponent(out Pickable pickable))
                    {
                        SetItemWeightCalculationAssociation(pickable.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                        {
                            foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                            {
                                if (roomThemeToBiomeAssociations.TryGetValue(roomData.m_room.m_theme, out Heightmap.Biome roomBiome) && (roomBiome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                {
                                    biome = biomeVal;
                                }
                            }

                            prevalence += (randomSpawn != null && randomSpawn.m_chanceToSpawn > 0f ? randomSpawn.m_chanceToSpawn / 100f : 1) * 0.2f;
                        });
                    }
                    else if (roomObject.TryGetComponent(out PickableItem pickableItem))
                    {
                        foreach (PickableItem.RandomItem randomPickableItem in pickableItem.m_randomItemPrefabs)
                        {
                            SetItemWeightCalculationAssociation(randomPickableItem.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                            {
                                foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                {
                                    if (roomThemeToBiomeAssociations.TryGetValue(roomData.m_room.m_theme, out Heightmap.Biome roomBiome) && (roomBiome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                    {
                                        biome = biomeVal;
                                    }
                                }

                                prevalence += (randomSpawn != null && randomSpawn.m_chanceToSpawn > 0f ? randomSpawn.m_chanceToSpawn / 100f : 1) * 0.2f;
                            });
                        }
                    }
                }
            }

            foreach (ZoneSystem.ZoneVegetation vegetation in ZoneSystem.instance.m_vegetation)
            {
                List<DropTable> dropTables = new List<DropTable>();

                if (vegetation.m_prefab.TryGetComponent(out Pickable pickable))
                {
                    SetItemWeightCalculationAssociation(pickable.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                    {
                        foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                        {
                            if ((vegetation.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                            {
                                biome = biomeVal;
                            }
                        }

                        prevalence += 1f;
                    });
                }
                else if (vegetation.m_prefab.TryGetComponent(out PickableItem pickableItem))
                {
                    foreach (PickableItem.RandomItem randomPickableItem in pickableItem.m_randomItemPrefabs)
                    {
                        SetItemWeightCalculationAssociation(randomPickableItem.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                        {
                            foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                            {
                                if ((vegetation.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                {
                                    biome = biomeVal;
                                }
                            }

                            prevalence += 1f;
                        });
                    }
                }
                else if (vegetation.m_prefab.TryGetComponent(out Plant plant))
                {
                    foreach (GameObject grownPrefab in plant.m_grownPrefabs)
                    {
                        if (grownPrefab.TryGetComponent(out Pickable pickableGrownPrefab))
                        {
                            SetItemWeightCalculationAssociation(pickableGrownPrefab.m_itemPrefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                            {
                                foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                {
                                    if ((vegetation.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                    {
                                        biome = biomeVal;
                                    }
                                }

                                prevalence += 1f;
                            });
                        }
                    }
                }
                else if (vegetation.m_prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed) && dropOnDestroyed.enabled)
                {
                    dropTables.Add(dropOnDestroyed.m_dropWhenDestroyed);
                }
                else if (vegetation.m_prefab.TryGetComponent(out IDestructible destructibleThing))
                {
                    if (destructibleThing is Destructible destructible && destructible.enabled)
                    {
                        if (destructible.m_spawnWhenDestroyed != null && destructible.m_spawnWhenDestroyed.TryGetComponent(out MineRock5 mineRock5))
                        {
                            dropTables.Add(mineRock5.m_dropItems);
                        }
                    }
                    else if (destructibleThing is TreeBase treeBase && treeBase.enabled)
                    {
                        dropTables.Add(treeBase.m_dropWhenDestroyed);

                        if (treeBase.m_logPrefab != null && treeBase.m_logPrefab.TryGetComponent(out TreeLog treeLog))
                        {
                            if (treeLog.m_subLogPrefab != null && treeLog.m_subLogPrefab.TryGetComponent(out TreeLog subTreeLog))
                            {
                                dropTables.Add(subTreeLog.m_dropWhenDestroyed);
                            }
                        }
                    }
                    else if (destructibleThing is MineRock mineRock && mineRock.enabled)
                    {
                        dropTables.Add(mineRock.m_dropItems);
                    }
                    else if (destructibleThing is MineRock5 mineRock5 && mineRock5.enabled)
                    {
                        dropTables.Add(mineRock5.m_dropItems);
                    }
                }

                foreach (DropTable dropTable in dropTables)
                {
                    if (dropTable.IsEmpty())
                    {
                        continue;
                    }

                    foreach (DropTable.DropData drop in dropTable.m_drops)
                    {
                        if (dropTable.m_dropChance == 0)
                        {
                            continue;
                        }

                        SetItemWeightCalculationAssociation(drop.m_item.name, (ref float prevalence, ref float biome, ref float enemy) =>
                        {
                            foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                            {
                                if ((vegetation.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                {
                                    biome = biomeVal;
                                }
                            }

                            prevalence += (dropTable.m_dropChance / 1f);
                        });
                    }
                }
            }

            foreach (SpawnSystemList spawnSystemList in ZoneSystem.instance.m_zoneCtrlPrefab.GetComponent<SpawnSystem>().m_spawnLists)
            {
                foreach (SpawnSystem.SpawnData spawnData in spawnSystemList.m_spawners)
                {
                    if (spawnData.m_prefab.TryGetComponent(out CharacterDrop characterDrop))
                    {
                        foreach (CharacterDrop.Drop drop in characterDrop.m_drops)
                        {
                            if (spawnData.m_prefab.TryGetComponent(out Humanoid humanoid))
                            {
                                SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                {
                                    if (enemy == 0)
                                    {
                                        float humanoidHealth = humanoid.m_nview != null ? humanoid.GetMaxHealth() : humanoid.m_health;
                                        ItemDrop.ItemData humanoidWeapon = humanoid.m_rightItem ?? humanoid.m_leftItem;
                                        enemy = Mathf.Pow((humanoidHealth + (humanoidWeapon != null ? humanoidWeapon.m_shared.m_damages.GetTotalDamage() : 0)) * humanoid.m_level * (humanoid.IsBoss() ? 5 : 1), enemyPricingWeight);
                                    }
                                });
                            }

                            if (spawnData.m_prefab.TryGetComponent(out Humanoid spawnHumanoid) && factionToBiomeAssociations.TryGetValue(spawnHumanoid.m_faction, out Heightmap.Biome spawnHumanoidBiome))
                            {
                                SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                {
                                    if (biomePricingWeights[spawnHumanoidBiome] < biome)
                                    {
                                        biome = biomePricingWeights[spawnHumanoidBiome];
                                    }
                                });
                            }
                            else
                            {
                                SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                {
                                    foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                    {
                                        if ((spawnData.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                        {
                                            biome = biomeVal;
                                        }
                                    }
                                });
                            }

                            SetItemWeightCalculationAssociation(drop.m_prefab.name, (ref float prevalence, ref float biome, ref float enemy) =>
                            {
                                prevalence += (drop.m_chance / 1f);
                            });
                        }
                    }
                }
            }

            /*foreach (GameObject itemObject in ObjectDB.instance.m_items)
            {
                if (itemObject.TryGetComponent(out ItemDrop drop) && drop.m_itemData?.m_shared?.m_buildPieces?.m_pieces != null)
                {
                    foreach (GameObject buildPiece in drop.m_itemData.m_shared.m_buildPieces.m_pieces)
                    {
                        if (buildPiece.TryGetComponent(out Plant plant))
                        {
                            foreach (GameObject grownPrefab in plant.m_grownPrefabs)
                            {
                                if (grownPrefab.TryGetComponent(out Pickable pickable))
                                {
                                    SetItemWeightCalculationAssociation(pickable.m_itemPrefab?.name, (ref float prevalence, ref float biome, ref float enemy) =>
                                    {
                                        foreach (Heightmap.Biome biomeFlag in Enum.GetValues(typeof(Heightmap.Biome)))
                                        {
                                            if ((plant.m_biome & biomeFlag) != 0 && biomePricingWeights.TryGetValue(biomeFlag, out int biomeVal) && biomeVal < biome)
                                            {
                                                biome = biomeVal;
                                            }
                                        }

                                        prevalence += Mathf.Pow(5f, -prevalence / 10f);
                                    });
                                }
                            }
                        }
                    }
                }
            }*/

            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe.m_item != null && !itemtoRecipeAssociations.ContainsKey(recipe.m_item.name))
                {
                    itemtoRecipeAssociations.Add(recipe.m_item.name, recipe);
                }
            }

            foreach (ITradableConfig config in configs)
            {
                if (!itemNameToConfigAssociations.ContainsKey(config.Drop.name))
                {
                    itemNameToConfigAssociations.Add(config.Drop.name, config);
                }
            }

            foreach (ITradableConfig config in configs)
            {
                if (config.Drop.m_itemData.m_shared.m_buildPieces != null)
                {
                    foreach (GameObject piece in config.Drop.m_itemData.m_shared.m_buildPieces.m_pieces)
                    {
                        Smelter smelter = null;
                        piece.TryGetComponent(out smelter);

                        if (smelter == null && piece.TryGetComponent(out Windmill windmill))
                        {
                            smelter = windmill.m_smelter;
                        }

                        if (smelter != null)
                        {
                            foreach (Smelter.ItemConversion conversion in smelter.m_conversion)
                            {
                                SetItemToNodeBuildPieceSetterAssociation(conversion.m_to.name, (configNode) =>
                                {
                                    configNode.isMadeFromSmelter = true;

                                    if (!conversion.m_from.name.ToLower().Contains("scrap") && itemNameToConfigAssociations.TryGetValue(conversion.m_from.name, out ITradableConfig conversionConfig))
                                    {
                                        TradableConfigNode conversionConfigNode = SetupConfigNodes(conversionConfig);

                                        if (!configNode.nodes.Any(node => node == conversionConfigNode))
                                        {
                                            conversionConfigNode.usefulness += Mathf.Pow(2, -conversionConfigNode.usefulness / 10f);
                                            configNode.nodes.Add(conversionConfigNode);
                                        }
                                    }
                                });
                            }
                        }
                        else if (piece.TryGetComponent(out CookingStation cookingStation))
                        {
                            foreach (CookingStation.ItemConversion cookingConversion in cookingStation.m_conversion)
                            {
                                if (itemNameToConfigAssociations.TryGetValue(cookingConversion.m_from.name, out ITradableConfig conversionConfig))
                                {
                                    SetItemToNodeBuildPieceSetterAssociation(cookingConversion.m_to.name, (configNode) =>
                                    {
                                        TradableConfigNode conversionConfigNode = SetupConfigNodes(conversionConfig);

                                        if (!configNode.nodes.Any(node => node == conversionConfigNode))
                                        {
                                            conversionConfigNode.usefulness++;
                                            configNode.nodes.Add(conversionConfigNode);
                                        }
                                    });
                                }
                            }
                        }
                        else if (piece.TryGetComponent(out Fermenter fermenter))
                        {
                            foreach (Fermenter.ItemConversion fermenterConversion in fermenter.m_conversion)
                            {
                                if (itemNameToConfigAssociations.TryGetValue(fermenterConversion.m_from.name, out ITradableConfig conversionConfig))
                                {
                                    SetItemToNodeBuildPieceSetterAssociation(fermenterConversion.m_to.name, (configNode) =>
                                    {
                                        TradableConfigNode conversionConfigNode = SetupConfigNodes(conversionConfig);

                                        if (!configNode.nodes.Any(node => node == conversionConfigNode))
                                        {
                                            conversionConfigNode.usefulness++;
                                            configNode.nodes.Add(conversionConfigNode);
                                        }
                                    });
                                }
                            }
                        }
                        else if (piece.TryGetComponent(out Plant plant))
                        {
                            if (!plant.name.ToLower().Contains("seed"))
                            {
                                if (plant.m_grownPrefabs.Length > 0 && plant.m_grownPrefabs[0] != null && plant.m_grownPrefabs[0].TryGetComponent(out Pickable produce))
                                {
                                    if (itemNameToConfigAssociations.TryGetValue(produce.m_itemPrefab.name, out ITradableConfig produceConfig))
                                    {
                                        if (plant.TryGetComponent(out Piece plantPiece))
                                        {
                                            if (plantPiece.m_resources.Length > 0 && plantPiece.m_resources[0] != null && plantPiece.m_resources[0].m_resItem != null)
                                            {
                                                if (itemNameToConfigAssociations.TryGetValue(plantPiece.m_resources[0].m_resItem.name, out ITradableConfig seedConfig))
                                                {
                                                    SetItemToNodeBuildPieceSetterAssociation(produceConfig.Drop.name, (configNode) =>
                                                    {
                                                        var conversionConfigNode = new TradableConfigNode(seedConfig);

                                                        if (!configNode.nodes.Any(node => node == conversionConfigNode))
                                                        {
                                                            conversionConfigNode.usefulness++;
                                                            configNode.nodes.Add(conversionConfigNode);
                                                        }
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (ITradableConfig config in configs)
            {
                SetupConfigNodes(config);
            }

            foreach (TradableConfigNode configNode in configNodes)
            {
                CalculatePurchasePrices(configNode);
            }

            EventManager.RaiseGeneratedConfigs(configs);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"bt.generate.config {maxStackPricingWeight} {weightPricingWeight} {teleportationPricingWeight} {prevalencePricingWeight} {usefulnessPricingWeight} {biomePricingWeight} {enemyPricingWeight} {globalWeight} {globalBasePricing}");
            sb.AppendLine();

            foreach (TradableConfigNode configNode in configNodes)
            {
                BuildTreeString(configNode, sb);
                sb.AppendLine();
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "generated-prices-out.txt"), sb.ToString());

            void BuildTreeString(TradableConfigNode configNode, StringBuilder sb, int indentation = 0, int spacingPerDepth = 30, int recipeQuantity = 0)
            {
                string formattedItemName = string.Format($"{{0,{indentation * spacingPerDepth}}}{{1}}", $"{configNode.config.Drop.name}{(recipeQuantity != 0 ? $" x{recipeQuantity}" : string.Empty)}{(configNode.recipe != null && indentation == 0 && recipeQuantity == 0 ? $" x{configNode.recipe.m_amount}" : string.Empty)}:", $"price({configNode.config.BasePurchasePrice}), maxStack({configNode.maxStack}), weight({configNode.weight}), teleportationPricingWeight({configNode.teleportation}) prevalence({configNode.prevalence}), biome({configNode.biome}), enemy({configNode.enemy}), usefulness({configNode.usefulness})");
                sb.AppendLine(formattedItemName);

                foreach (TradableConfigNode node in configNode.nodes)
                {
                    BuildTreeString(node, sb, indentation + 1, spacingPerDepth, configNode.recipe != null ? configNode.recipe.m_resources.Sum(resource => resource.m_resItem.name == node.config.Drop.name ? resource.m_amount : 0) : 0);
                }
            }

            TradableConfigNode SetupConfigNodes(ITradableConfig config)
            {
                if (configToNodeAssociations.TryGetValue(config, out TradableConfigNode existingConfigNode))
                {
                    return existingConfigNode;
                }

                var configNode = new TradableConfigNode(config);
                configNodes.Add(configNode);
                configToNodeAssociations.Add(config, configNode);

                if (itemtoRecipeAssociations.TryGetValue(config.Drop.name, out Recipe recipe))
                {
                    if (recipe.m_item?.name == config.Drop.name)
                    {
                        configNode.recipe = recipe;

                        foreach (Piece.Requirement recipeRequirement in recipe.m_resources)
                        {
                            if (recipeRequirement.m_amount == 0)
                            {
                                continue;
                            }

                            if (itemNameToConfigAssociations.TryGetValue(recipeRequirement.m_resItem.name, out ITradableConfig requirementConfig))
                            {
                                TradableConfigNode requirementConfigNode = SetupConfigNodes(requirementConfig);

                                if (!configNode.nodes.Any(node => node == requirementConfigNode))
                                {
                                    requirementConfigNode.usefulness++;
                                    configNode.nodes.Add(requirementConfigNode);
                                }
                            }
                        }
                    }
                }

                if (itemToNodeBuildPieceSetterAssociations.TryGetValue(config.Drop.name, out NodeBuildPieceSetter nodeBuildPieceSetter))
                {
                    nodeBuildPieceSetter(configNode);
                }

                return configNode;
            }

            int CalculatePurchasePrices(TradableConfigNode configNode)
            {
                if (completedConfigNodes.Contains(configNode))
                {
                    return configNode.config.BasePurchasePrice;
                }

                float purchasePrice = configNode.nodes.Count == 0 ? globalBasePricing : 0;
                float prevalence = 0;
                float biome = float.MaxValue;
                float enemy = 0;
                float maxStack = Mathf.Pow(2, -configNode.config.Drop.m_itemData.m_shared.m_maxStackSize * maxStackPricingWeight);
                float weight = Mathf.Pow(configNode.config.Drop.m_itemData.m_shared.m_weight, weightPricingWeight);
                float teleportation = !configNode.config.Drop.m_itemData.m_shared.m_teleportable ? Mathf.Pow(2, teleportationPricingWeight) : 1;
                float usefulness = Mathf.Pow(configNode.usefulness, usefulnessPricingWeight);
                float sumOfPartPrices = float.MinValue;

                foreach (TradableConfigNode node in configNode.nodes)
                {
                    int partPrice = CalculatePurchasePrices(node);

                    if (configNode.isMadeFromSmelter || (configNode.recipe != null && configNode.recipe.m_requireOnlyOneIngredient))
                    {
                        if (node.prevalence != 0)
                        {
                            sumOfPartPrices = Mathf.Max(sumOfPartPrices, partPrice);
                        }
                    }
                    else
                    {
                        if (sumOfPartPrices == float.MinValue)
                        {
                            sumOfPartPrices = 0;
                        }

                        sumOfPartPrices += partPrice * (configNode.recipe != null ? configNode.recipe.m_resources.Sum(piece => piece.m_resItem?.name == node.config.Drop.name ? piece.m_amount : 0) : 1);
                    }
                }

                if (sumOfPartPrices != float.MinValue)
                {
                    purchasePrice += sumOfPartPrices;
                }

                if (itemToWeightCalculationAssociations.TryGetValue(configNode.config.Drop.name, out PriceCalculator actionRef))
                {
                    actionRef(ref prevalence, ref biome, ref enemy);
                }

                bool biomeSetToRecipeIngredient = false;

                if (biome == float.MaxValue)
                {
                    biome = float.MinValue;

                    foreach (TradableConfigNode node in configNode.nodes)
                    {
                        if (node.biome > biome)
                        {
                            biome = node.biome;
                            biomeSetToRecipeIngredient = true;
                        }
                    }
                }

                if (!biomeSetToRecipeIngredient)
                {
                    biome = (biome != float.MinValue && biome != float.MaxValue) ? Mathf.Pow(biome, biomePricingWeight) : 1;
                }

                prevalence = prevalence > 0f ? Mathf.Pow(4, -prevalence * prevalencePricingWeight) : 0;

                if (configNode.nodes.Count == 0)
                {
                    purchasePrice *= prevalence > 0f ? prevalence : 1f;
                    purchasePrice *= maxStack > 0f ? maxStack : 1f;
                    purchasePrice *= weight > 0f ? weight : 1f;
                    purchasePrice *= teleportation;
                    purchasePrice *= biome > 0f ? biome : 1f;
                    purchasePrice *= enemy > 0f ? enemy : 1f;
                    purchasePrice *= usefulness > 0f ? usefulness : 1f;
                    purchasePrice *= globalWeight;
                }

                purchasePrice /= configNode.recipe != null ? configNode.recipe.m_amount : 1f;
                configNode.prevalence = prevalence > 0f ? prevalence : configNode.nodes.Sum(node => node.prevalence);
                configNode.maxStack = maxStack;
                configNode.weight = weight;
                configNode.teleportation = teleportation;
                configNode.biome = biome;
                configNode.enemy = enemy;
                configNode.usefulness = usefulness;
                configNode.config.BasePurchasePrice = (int)purchasePrice;
                configNode.config.BaseSalesPrice = (int)(purchasePrice * 0.5f);
                completedConfigNodes.Add(configNode);

                return (int)purchasePrice;
            }
            
            void SetItemWeightCalculationAssociation(string key, PriceCalculator actionRef)
            {
                if (itemToWeightCalculationAssociations.TryGetValue(key, out PriceCalculator existingActionRef))
                {
                    itemToWeightCalculationAssociations[key] = (PriceCalculator)Delegate.Combine(actionRef, existingActionRef);
                }
                else
                {
                    itemToWeightCalculationAssociations.Add(key, actionRef);
                }
            }

            void SetItemToNodeBuildPieceSetterAssociation(string key, NodeBuildPieceSetter actionRef)
            {
                if (itemToNodeBuildPieceSetterAssociations.TryGetValue(key, out NodeBuildPieceSetter existingActionRef))
                {
                    itemToNodeBuildPieceSetterAssociations[key] = (NodeBuildPieceSetter)Delegate.Combine(actionRef, existingActionRef);
                }
                else
                {
                    itemToNodeBuildPieceSetterAssociations.Add(key, actionRef);
                }
            }

            float GetEnemyValue(Humanoid humanoid)
            {
                float humanoidHealth = humanoid.m_nview != null ? humanoid.GetMaxHealth() : humanoid.m_health;
                ItemDrop.ItemData humanoidWeapon = humanoid.m_rightItem ?? humanoid.m_leftItem;
                
                return Mathf.Pow((humanoidHealth + (humanoidWeapon != null ? humanoidWeapon.m_shared.m_damages.GetTotalDamage() : 0)) * humanoid.m_level * (humanoid.IsBoss() ? 10f : 1), enemyPricingWeight);
            }
        }
    }
}
