using System.IO;
using System.Threading.Tasks;
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

        public static object DeserializeStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
            {
                return serializer.Deserialize(jsonReader);
            }
        }
    }
}