using System.IO;
using Newtonsoft.Json;

namespace Communication.JsonSerializerExtensions
{
    public static class Utility
    {
        public static T DeserializeStream<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}