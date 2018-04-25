using System;
using System.IO;
using Newtonsoft.Json;

namespace Communication.JsonSerializerExtensions
{
    public static class Utility
    {
        public static T DeserializeStream<T>(Stream stream, JsonSerializer serializer = null)
        {
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                if (serializer == null) serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static object DeserializeStream(Stream stream, Type type, JsonSerializer serializer = null)
        {
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                if (serializer == null) serializer = new JsonSerializer();
                return serializer.Deserialize(jsonReader, type);
            }
        }

        public static object DeserializeStream(Stream stream, JsonSerializer serializer = null)
        {
            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                if (serializer == null) serializer = new JsonSerializer();
                return serializer.Deserialize(jsonReader);
            }
        }
    }
}