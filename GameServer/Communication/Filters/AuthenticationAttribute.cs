using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Communication.JsonConverter;
using Domain.Models;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Communication.Filters
{
    /// <summary>
    /// Provides authentication for requests. 
    /// Note that this requires <see cref="FromDtoModelBinder"/> as the model binder
    /// </summary>
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public ILoginManager LoginManager => Application.Managers.LoginManager.GetInstance();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // Check if actionsDescriptions contains authentication
                var containsAuthentication = filterContext.ActionDescriptor.Properties.ContainsKey("auth");

                if (containsAuthentication)
                {
                    var msg = filterContext.ActionDescriptor.Properties["auth"] as string;

                    // If type of value is a user fetch this
                    User confirmedUser = (User)filterContext.ActionArguments.Values.FirstOrDefault(o => (o.GetType() == typeof(User)));

                    // Get json dictionary
                    var jsonObj = JObject.Parse(msg);

                    // Fetch username and password
                    var user = jsonObj["username"].ToString();
                    var pass = jsonObj["password"].ToString();

                    // If value type was a user compare username to passed username to make sure it's the same user
                    // Then check login status
                    if (confirmedUser != null)
                    {
                        if (confirmedUser.Username == user)
                        {
                            if (LoginManager.CheckLoginStatus(user, pass))
                            {
                                return;
                            }
                        }
                    }
                    else /* If not then check the login status */
                    {
                        if (LoginManager.CheckLoginStatus(user, pass))
                        {
                            return;
                        }
                    }
                   
                }
            }
            catch (Exception)
            {
                
            }

            /* If we reach this point the request is unauthorized */
            filterContext.Result = new UnauthorizedResult();
        }
    }
}