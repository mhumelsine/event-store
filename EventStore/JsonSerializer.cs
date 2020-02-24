using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public class JsonSerializer : ISerializer
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public string Serialize<T>(T aggregate)
        {
            return JsonConvert.SerializeObject(aggregate, settings);
        }

        public T Deserialize<T>(string serializedObject)
        {
            return (T)JsonConvert.DeserializeObject(serializedObject, settings);
        }
    }
}
