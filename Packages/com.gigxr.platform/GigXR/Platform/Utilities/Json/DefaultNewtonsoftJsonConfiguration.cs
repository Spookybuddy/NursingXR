using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;
using UnityEngine;

namespace GIGXR.Utilities
{
    /// <summary>
    /// This class is used to configure the Newtonsoft JSON serializer settings.
    /// 
    /// <a href="https://www.newtonsoft.com/json/help/html/SerializationSettings.htm">See more settings</a>
    /// </summary>
    public static class DefaultNewtonsoftJsonConfiguration
    {
        private static bool defaultJsonConvertSet = false;

        [RuntimeInitializeOnLoadMethod]
        private static JsonSerializerSettings Initialize()
        {
            // Disable adding to DefaultSettings because we will do so below.
            UnityConverterInitializer.shouldAddConvertsToDefaultSettings = false;

            var settings = UnityConverterInitializer.defaultUnityConvertersSettings;
            settings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
            settings.ContractResolver = new UnityTypeAndCamelCasePropertyNameContractResolver();
            settings.Formatting = Formatting.Indented;
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;

            if(!defaultJsonConvertSet)
            {
                defaultJsonConvertSet = true;

                JsonConvert.DefaultSettings = () =>
                {
                    return settings;
                };
            }

            if(_jsonSerializer == null)
                _jsonSerializer = CreateSerializer(settings);

            return settings;
        }

        private static JsonSerializer CreateSerializer(JsonSerializerSettings settings)
        {
            var serializer = JsonSerializer.Create(settings);

            return serializer;
        }

        private static JsonSerializerSettings _serializerSettings;

        public static JsonSerializerSettings SerializerSettings => _serializerSettings ?? (_serializerSettings = Initialize());

        public static JsonSerializer _jsonSerializer;

        public static JsonSerializer JsonSerializer => _jsonSerializer ?? (_jsonSerializer = CreateSerializer(SerializerSettings));
    }
}