using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Communication.JsonConverter;
using Communication.ModelBinders;
using Domain.Models;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace Communication.Filters
{
    /// <summary>
    /// Provides authentication for requests. 
    /// Note that this requires <see cref="FromDto"/> as the model binder
    /// </summary>
    public class AuthenticationAttribute : ActionFilterAttribute
    {
        public ILoginManager LoginManager => Application.Managers.LoginManager.GetInstance();

        private const string AuthenticationDelimeter = "auth";

        /// <summary>
        /// Property name that controllers can use to fetch authentication token
        /// </summary>
        private const string ControllerAuthToken = "authtoken"; 
                                    
        private const string UserCredentials = "username";
        private const string KeyCredentials = "password";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // Check if actionsDescriptions contains authentication
                var containsAuthentication = filterContext.ActionDescriptor.Properties.ContainsKey(AuthenticationDelimeter);

                if (containsAuthentication)
                {
                    var msg = filterContext.ActionDescriptor.Properties[AuthenticationDelimeter] as Dictionary<string, string>;

                    // Fetch username and password
                    var user = msg[UserCredentials];
                    var pass = msg[KeyCredentials];

                    // If value type was a user compare username to passed username to make sure it's the same user
                    if (LoginManager.CheckLoginStatus(user, pass))
                    {
                        SetController(filterContext.Controller, msg);
                        return;
                    }                           
                }
            }
            catch (Exception)
            {
                
            }

            /* If we reach this point the request is unauthorized */
            filterContext.Result = new UnauthorizedResult();
        }

        private static void SetController(object controller, Dictionary<string, string> auth)
        {
            var controllerProperties = controller.GetType().GetProperties();

            var targetProp = controllerProperties.
                FirstOrDefault(prop => prop.Name.ToLower().Contains(ControllerAuthToken) && prop.PropertyType == auth.GetType());

            if (targetProp != null)
            {
                targetProp.SetValue(controller, auth);
            }
        }
    }
}