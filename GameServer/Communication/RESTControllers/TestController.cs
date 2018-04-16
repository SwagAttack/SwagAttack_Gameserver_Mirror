using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;

namespace Communication.RESTControllers
{
    [Produces("application/json")]
    [Route("api/Test")]
    public class TestController : Controller
    {
        [HttpPost]
        public IActionResult Post([FromBody] JObject data)
        {
            if (data.Count != 2)
                return BadRequest();

            if (!data.ContainsKey("auth"))
            {
                Debug.WriteLine("Wrong");
            }

            if (data.ContainsKey("val"))
            {
                IUser user = data.GetValue("val").ToObject<Domain.Models.User>();

                var test = GeneretateObject(user.Username, user.Password, user);
                return new ObjectResult(test);
            }

            return new ObjectResult(data);
        }


        public static JObject GeneretateObject<T>(string user, string pass, T obj) 
        {
            JObject target = new JObject();

            Dictionary<string, string> auth = new Dictionary<string, string>();

            auth.Add("id", user);
            auth.Add("pass", pass);
            target.Add("auth", JObject.FromObject(auth));

            target.Add("val", JObject.FromObject(obj));
            return target;
        }
    }

}