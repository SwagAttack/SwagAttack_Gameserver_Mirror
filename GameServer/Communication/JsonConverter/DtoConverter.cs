using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Communication.JsonConverter
{
    /// <summary>
    /// Unnecessary class. Is more valueble client side.
    /// </summary>
    public class DtoConverter
    {
        /// <summary>
        /// Converts the given JObject to a contrete type. May throw!
        /// </summary>
        /// <typeparam name="T"> Concrete instance of which to convert the JObject to </typeparam>
        /// <param name="obj"></param>
        /// <returns> The converted output </returns>
        public static T ConvertToInstance<T>(JObject obj)
        {
            var target = obj.GetValue("val");
            T result = target.ToObject<T>();
            return result;
        }

        public static object ConvertToInstance(JObject obj, Type type)
        {
            var target = obj.GetValue("val");
            object result = target.ToObject(type);
            return result;
        }

        /// <summary>
        /// For getting the authentication information from a JObject
        /// </summary>
        /// <param name="obj"> The object to read authentication from </param>
        /// <returns> The dictionary containing authentication information or null on error </returns>
        public static Dictionary<string, string> GetAuthentication(JObject obj)
        {
            try
            {
                var target = obj.GetValue("auth");
                var result = target.ToObject<Dictionary<string, string>>();
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// For converting the given object to a DTO object
        /// </summary>
        /// <typeparam name="T"> _binderType to convert to DTO </typeparam>
        /// <param name="username"> Username authentication </param>
        /// <param name="password"> Password authentication </param>
        /// <param name="t"> Object to convert </param>
        /// <returns> The DTO representing the object </returns>
        public static JObject ConvertToDto<T>(string username, string password, T t) where T : class
        {
            JObject target = new JObject();

            var auth = new Dictionary<string, string>();
            auth.Add("id", username);
            auth.Add("pass", password);

            target.Add("auth", JObject.FromObject(auth));
            target.Add("val", JObject.FromObject(t));

            return target;
        }
    }
}