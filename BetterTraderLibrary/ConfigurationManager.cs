using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public sealed class ConfigurationManager
    {
        private readonly string traderConfigFilePath;
        private readonly string tradableItemConfigFolderPath;
        private readonly Dictionary<string, ISerializer> serializersByFileExtension;
        private FileSystemWatcher traderConfigWatcher;
        private readonly List<FileSystemWatcher> itemConfigWatchers = new List<FileSystemWatcher>();

        #region Custom Exceptions
        private class BTLoadTraderException : Exception
        {
            public BTLoadTraderException(string message) : base(message) { }
            public BTLoadTraderException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class BTLoadTradableItemException : Exception
        {
            public BTLoadTradableItemException(string message) : base(message) { }
            public BTLoadTradableItemException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class BTSaveTraderException : Exception
        {
            public BTSaveTraderException(string message) : base(message) { }
            public BTSaveTraderException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class BTSaveTradableItemException : Exception
        {
            public BTSaveTradableItemException(string message) : base(message) { }
            public BTSaveTradableItemException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class BTGetSerializerException : Exception
        {
            public BTGetSerializerException(string message) : base(message) { }
            public BTGetSerializerException(string message, Exception innerException) : base(message, innerException) { }
        }
        #endregion

        public ConfigurationManager(string traderConfigFilePath, string tradableItemConfigFolderPath, Dictionary<string, ISerializer> serializersByFileExtension)
        {
            this.traderConfigFilePath = traderConfigFilePath;
            this.tradableItemConfigFolderPath = tradableItemConfigFolderPath;
            this.serializersByFileExtension = serializersByFileExtension;
            Directory.CreateDirectory(tradableItemConfigFolderPath);
        }

        public BTrader LoadTrader<T>() where T : ITradableConfig
        {
            BTrader trader = null;

            if (File.Exists(traderConfigFilePath))
            {
                ISerializer serializer = GetSerializerBasedOnFile(traderConfigFilePath);
                string traderConfiguration = File.ReadAllText(traderConfigFilePath);

                try
                {
                    trader = serializer.Deserialize<BTrader>(traderConfiguration);
                }
                catch (Exception e)
                {
                    throw new BTLoadTraderException($"Failed to deserialize configuration file [{Path.GetFileName(traderConfigFilePath)}]! Double check the file and ensure there are no errors before trying again!", e);
                }
            }

            if (trader != null)
            {
                trader.ItemConfigurations = LoadItems<T>();
            }

            return trader;
        }

        private List<ITradableConfig> LoadItems<T>() where T : ITradableConfig
        {
            ConcurrentBag<ITradableConfig> loadedItems = new ConcurrentBag<ITradableConfig>();
            string[] tradableItemConfigFilePaths = Directory.GetFiles(tradableItemConfigFolderPath);

            // Loads all items from existing configs into a collection.
            Parallel.ForEach(tradableItemConfigFilePaths, (tradableItemConfigFilePath, _, _) =>
            {
                ISerializer serializer = GetSerializerBasedOnFile(tradableItemConfigFilePath);
                string tradableItemConfiguration = File.ReadAllText(tradableItemConfigFilePath);
                List<T> config;

                try
                {
                    config = serializer.Deserialize<List<T>>(tradableItemConfiguration);
                }
                catch (Exception e)
                {
                    throw new BTLoadTradableItemException($"Failed to deserialize configuration file [{Path.GetFileName(tradableItemConfigFilePath)}]! Double check the file as well as path, and ensure there are no errors before trying again!", e);
                }

                config.ForEach(item => loadedItems.Add(item));
            });

            return loadedItems.ToList();
        }

        public void Save(BTrader trader)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(traderConfigFilePath) ?? throw new InvalidOperationException());
            ISerializer serializer;
            string traderConfigOutputPath = traderConfigFilePath;
            string[] traderConfigFileSearchResults = Directory.GetFiles(Path.GetDirectoryName(traderConfigFilePath) ?? throw new InvalidOperationException(), "trader.*");
            string existingTraderConfigFilePath = traderConfigFilePath;

            if (traderConfigFileSearchResults.Length != 0)
            {
                existingTraderConfigFilePath = traderConfigFileSearchResults[0];
            }

            // If there is already a trader config file present, choose that as the output path so that the correct serializer is chosen based on the file extension.
            if (!string.IsNullOrEmpty(existingTraderConfigFilePath))
            {
                traderConfigOutputPath = existingTraderConfigFilePath;
            }

            serializer = GetSerializerBasedOnFile(traderConfigOutputPath);
            string traderConfig;

            try
            {
                traderConfig = serializer.Serialize(trader);
            }
            catch (Exception e)
            {
                throw new BTSaveTraderException("Failed to serialize Trader data!", e);
            }

            File.WriteAllText(traderConfigOutputPath, traderConfig);
        }

        public void Save(List<ITradableConfig> items)
        {
            string[] itemConfigFilePaths = Directory.GetFiles(tradableItemConfigFolderPath);
            ConcurrentDictionary<string, string> itemConfigsFileByName = new ConcurrentDictionary<string, string>();

            // Items that will override existing config values.
            ConcurrentDictionary<string, ConcurrentBag<ITradableConfig>> itemConfigsByConfig = new ConcurrentDictionary<string, ConcurrentBag<ITradableConfig>>();

            // Items that don't have configurations yet, which will be saved to an auxilliary file.
            ConcurrentBag<ITradableConfig> leftOverConfigs = new ConcurrentBag<ITradableConfig>();

            // Creates "item name -> config file path" associations for all item configs.
            Parallel.ForEach(itemConfigFilePaths, (itemConfigFilePath) =>
            {
                string tradableItemConfig = File.ReadAllText(itemConfigFilePath);
                ISerializer serializer = GetSerializerBasedOnFile(itemConfigFilePath);
                List<ITradableConfig> loadedItems;

                try
                {
                    loadedItems = serializer.Deserialize<List<ITradableConfig>>(tradableItemConfig);
                }
                catch(Exception e)
                {
                    throw new BTSaveTradableItemException($"Failed to deserialize configuration file [{Path.GetFileName(itemConfigFilePath)}]! Double check the file as well as path, and ensure there are no errors before trying again!", e);
                }

                loadedItems.ForEach(item =>
                {
                    if (!itemConfigsFileByName.TryAdd(item.Name, itemConfigFilePath))
                    {
                        throw new BTSaveTradableItemException($"Failed to make config file to item name association!");
                    }
                });
            });

            // For each item currently being saved, see if it exists in any of the config files, and if it does, store it in a collection
            // to overwrite it later. Otherwise, store it in a separate collection to be saved in an auxilliary file.
            Parallel.ForEach(items, (item) =>
            {
                if (itemConfigsFileByName.TryGetValue(item.Name, out string associatedConfigFile))
                {
                    if (itemConfigsByConfig.TryGetValue(associatedConfigFile, out ConcurrentBag<ITradableConfig> configs))
                    {
                        configs.Add(item);
                    }
                    else
                    {
                        ConcurrentBag<ITradableConfig> newConfig = new ConcurrentBag<ITradableConfig>
                        {
                            item
                        };
                        
                        if (!itemConfigsByConfig.TryAdd(associatedConfigFile, newConfig))
                        {
                            throw new BTSaveTradableItemException($"Failed to add item to config file [{associatedConfigFile}]!");
                        }
                    }
                }
                else
                {
                    leftOverConfigs.Add(item);
                }
            });

            // Overwrites existing configurations with new data.
            foreach(KeyValuePair<string, ConcurrentBag<ITradableConfig>> config in itemConfigsByConfig)
            {
                ISerializer serializer = GetSerializerBasedOnFile(config.Key);
                string configFileContents = serializer.Serialize(config.Value.ToList());
                File.WriteAllText(config.Key, configFileContents);
            }

            // Creates configurations for items that don't have one yet. All saved in a common auxilliary file.
            string tmpItemConfigFilePath = Path.Combine(tradableItemConfigFolderPath, "items.yml");
            ISerializer tmpItemConfigSerializer = GetSerializerBasedOnFile(tmpItemConfigFilePath);
            string leftOverItemsConfig = tmpItemConfigSerializer.Serialize(leftOverConfigs.ToList());
            File.AppendAllText(tmpItemConfigFilePath, leftOverItemsConfig);
        }

        public void GenerateDefaultTraderConfig()
        {
            BTrader trader = LoadTrader<Item>() ?? new BTrader();
            Save(trader);
        }

        public void GenerateDefaultItemConfigs<T>(List<ITradableConfig> items) where T : ITradableConfig
        {
            // Used as an ad-hock, thread-safe hashmap.
            ConcurrentDictionary<string, byte> itemsByName = new ConcurrentDictionary<string, byte>();
            List<ITradableConfig> existingItems = LoadItems<T>();
            ConcurrentBag<ITradableConfig> itemsWithoutConfigs = new ConcurrentBag<ITradableConfig>();

            // Caching the names of items that have configs already.
            Parallel.ForEach(existingItems, item =>
            {
                itemsByName.TryAdd(item.Name, 0);
            });

            // Collecting items that don't have configs.
            Parallel.ForEach(items, item =>
            {
                if (!itemsByName.ContainsKey(item.Name))
                {
                    itemsWithoutConfigs.Add(item);
                }
            });

            if (itemsWithoutConfigs.Count == 0)
            {
                return;
            }

            string tmpItemConfigFilePath = Path.Combine(tradableItemConfigFolderPath, "items.yml");
            ISerializer tmpItemConfigSerializer = GetSerializerBasedOnFile(tmpItemConfigFilePath);
            string leftOverItemsConfig = tmpItemConfigSerializer.Serialize(itemsWithoutConfigs.ToList());
            File.AppendAllText(tmpItemConfigFilePath, leftOverItemsConfig);
        }

        public void SetupFileWatchers(Action<object, FileSystemEventArgs> traderConfigChanged)
        {
            if (traderConfigWatcher == null)
            {
                traderConfigWatcher = new FileSystemWatcher();
                traderConfigWatcher.Path = Path.GetDirectoryName(traderConfigFilePath);
                traderConfigWatcher.NotifyFilter = NotifyFilters.LastWrite;
                traderConfigWatcher.Changed += new FileSystemEventHandler(traderConfigChanged);
                traderConfigWatcher.EnableRaisingEvents = true;
            }
        }

        private ISerializer GetSerializerBasedOnFile(string filePath)
        {
            string fileExtension = Path.GetExtension(filePath);

            if (serializersByFileExtension.TryGetValue(fileExtension, out ISerializer serializer))
            {
                return serializer;
            }

            throw new BTGetSerializerException($"Could not find appropriate serializer for {Path.GetFileName(filePath)}! Make sure there's no error in the file's extension and that the extension is supported by BT!");
        }
    }
}
