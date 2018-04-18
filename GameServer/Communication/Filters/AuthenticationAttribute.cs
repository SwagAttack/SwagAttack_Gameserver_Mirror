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

        private readonly string _authenticationDelimeter = "auth";

        /// <summary>
        /// Property name that controllers can use to fetch authentication token
        /// </summary>
        private readonly string _controllerAuthToken = "authtoken"; 
                                    
        private readonly string _userCredentials = "username";
        private readonly string _keyCredentials = "password";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                // Check if actionsDescriptions contains authentication
                var containsAuthentication = filterContext.ActionDescriptor.Properties.ContainsKey(_authenticationDelimeter);

                if (containsAuthentication)
                {
                    var msg = filterContext.ActionDescriptor.Properties[_authenticationDelimeter] as Dictionary<string, string>;

                    // Fetch username and password
                    var user = msg[_userCredentials];
                    var pass = msg[_keyCredentials];

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

        private void SetController(object controller, Dictionary<string, string> auth)
        {
            var controllerProperties = controller.GetType().GetProperties();
            PropertyInfo targetProp = null;
            foreach (var prop in controllerProperties)
            {
                if (prop.Name.ToLower().Contains(_controllerAuthToken) && prop.PropertyType == auth.GetType())
                {
                    targetProp = prop;
                    break;
                }                   
            }
            if (targetProp != null)
            {
                targetProp.SetValue(controller, auth);
            }
        }
    }
}