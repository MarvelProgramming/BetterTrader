using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Menthus15Mods.Valheim.BetterTraderLibrary
{
    public class YamlConfigurationSerializer : IConfigurationSerializer
    {
        public T Deserialize<T>(string input)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            T deserializedObj = deserializer.Deserialize<T>(@input);

            return deserializedObj;
        }

        public string Serialize(object obj)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string serializedObj = @serializer.Serialize(obj);

            return serializedObj;
        }
    }
}
