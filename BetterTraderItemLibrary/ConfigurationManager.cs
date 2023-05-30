using Menthus15Mods.Valheim.BetterTraderLibrary.Extensions;
using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using Menthus15Mods.Valheim.BetterTraderLibrary.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class ConfigurationManager<T, U> : IConfigurationManager<T, U> where T : ITradable, new() where U : new()
    {
        private readonly IConfigurationSerializer configurationSerializer;

        public ConfigurationManager(IConfigurationSerializer configurationSerializer)
        {
            this.configurationSerializer = configurationSerializer;
        }

        private class LoadingTraderException : Exception
        {
            public LoadingTraderException(string message, Exception innerException) : base(message, innerException) { }
        }

        private class LoadingItemsException : Exception
        {
            public LoadingItemsException(string message, Exception innerException) : base(message, innerException) { }
        }

        public U LoadTrader()
        {
            try
            {
                var traderConfigurationFilePath = File.ReadAllText(Path.Combine(ConfigurationUtils.ConfigPath, Properties.Settings.Default.TraderConfigurationFileName + ".yaml"));
                var traderConfiguration = configurationSerializer.Deserialize<U>(traderConfigurationFilePath);

                return traderConfiguration;
            }
            catch (Exception e)
            {
                throw new LoadingTraderException($"An error occured while loading trader configuration file! Check that you have it at [{ConfigurationUtils.ConfigPath}] and make sure there aren't any typos in it", e);
            }
        }

        /// <summary>
        /// Loads all of the configurations for items across all config files
        /// </summary>
        public List<T> LoadItems()
        {
            try
            {
                var configFiles = Directory.GetFiles(Path.Combine(ConfigurationUtils.ConfigPath, "items"));
                var readTasks = GetReadTasks(configFiles);
                Task.WaitAll(readTasks);
                var loadedItems = ProcessItemReadTaskResults(readTasks, configFiles);

                return loadedItems;
            }
            catch (Exception e)
            {
                throw new LoadingItemsException($"An error occured while loading one or more item configuration files! Check your item configurations at [{Path.Combine(ConfigurationUtils.ConfigPath, "items")}] for any errors", e);
            }
        }

        public void CreateDefaultTraderConfigurationFile()
        {
            var trader = new U();
            Directory.CreateDirectory(ConfigurationUtils.ConfigPath);
            File.WriteAllText(Path.Combine(ConfigurationUtils.ConfigPath, Properties.Settings.Default.TraderConfigurationFileName + ".yaml"), configurationSerializer.Serialize(trader));
        }

        public void CreateDefaultItemConfigurationFiles(List<T> tradableItems)
        {
            var categorizedDefaultItemConfigs = GetCategorizedDefaultItemConfigs(tradableItems);

            if (categorizedDefaultItemConfigs.Count == 0)
            {
                return;
            }

            Debug.Log("Creating default configs...");
            var itemsConfigurationPath = Path.Combine(ConfigurationUtils.ConfigPath, "items");
            Directory.CreateDirectory(itemsConfigurationPath);

            foreach (var category in categorizedDefaultItemConfigs)
            {
                if (category.Value.Count == 0)
                {
                    continue;
                }

                Debug.Log($"Writing {category.Value.Count} items to {category.Key} config...");
                var newConfigFilePath = Path.Combine(itemsConfigurationPath, $"Vanilla.{category.Key}.yaml");
                var configFileContent = configurationSerializer.Serialize(categorizedDefaultItemConfigs[category.Key]);
                File.WriteAllText(newConfigFilePath, configFileContent);
            }
        }

        private Dictionary<string, List<T>> GetCategorizedDefaultItemConfigs(List<T> tradableItems)
        {
            var categorizedDefaultItemConfigs = new Dictionary<string, List<T>>();

            foreach (var item in tradableItems)
            {
                string itemType = ObjectDBUtils.GetDropFromItemName(item.Name).GetCustomCategory();

                if (!categorizedDefaultItemConfigs.ContainsKey(itemType))
                {
                    categorizedDefaultItemConfigs.Add(itemType, new List<T>());
                }

                categorizedDefaultItemConfigs[itemType].Add(item);
            }

            return categorizedDefaultItemConfigs;
        }

        private Task<T[]>[] GetReadTasks(string[] configFiles)
        {
            var readTasks = new Task<T[]>[configFiles.Length];

            for (int i = 0; i < configFiles.Length; i++)
            {
                var config = configFiles[i];
                var readTask = Task.Run(() => ReadItemConfig(config));
                readTasks[i] = readTask;
            }

            return readTasks;
        }

        private T[] ReadItemConfig(string config)
        {
            T[] configItems;

            try
            {
                var content = File.ReadAllText(config);
                configItems = configurationSerializer.Deserialize<T[]>(@content);
            }
            catch (Exception e)
            {
                throw new Exception($"Error while deserializing {config}!", e);
            }

            return configItems;
        }

        private List<T> ProcessItemReadTaskResults(Task<T[]>[] readTasks, string[] configFiles)
        {
            List<T> items = new List<T>();

            for (int i = 0; i < readTasks.Length; i++)
            {
                var readTask = readTasks[i];
                var config = Path.GetFileName(configFiles[i]);
                var itemConfigurationsReadFromTask = readTask.Result;

                if (itemConfigurationsReadFromTask.Length > 0)
                {
                    items.AddRange(itemConfigurationsReadFromTask);
                    Debug.Log($"Successfully loaded {itemConfigurationsReadFromTask.Length} items from {config}");
                }
                else
                {
                    Debug.Log($"Either failed to load [{config}] config or there were no items in it. Skipping...");
                }
            }

            return items;
        }
    }
}
