using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Communication.JsonConverter;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Communication.Filters
{
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public ILoginManager LoginManager => Application.Managers.LoginManager.GetInstance();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var msg = filterContext.ActionArguments.Values.ToList()[0] as JObject;

                Dictionary<string, string> authentication;
                if ((authentication = DtoConverter.GetAuthentication(msg)) != null)
                {
                    var user = authentication["username"];
                    var pass = authentication["password"];

                    if (LoginManager.CheckLoginStatus(user, pass))
                    {
                        return;
                    }

                }

            }
            catch (Exception)
            {
                
            }

            filterContext.Result = new UnauthorizedResult();
        }
    }
}