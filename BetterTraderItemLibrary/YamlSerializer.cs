using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class YamlSerializer : Interfaces.ISerializer
    {
        public T Deserialize<T>(string obj)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            T deserializedObj = deserializer.Deserialize<T>(obj);

            return deserializedObj;
        }

        public string Serialize<T>(T obj)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithIndentedSequences()
                .Build();
            string serializedObj = serializer.Serialize(obj);

            return serializedObj;
        }
    }
}
