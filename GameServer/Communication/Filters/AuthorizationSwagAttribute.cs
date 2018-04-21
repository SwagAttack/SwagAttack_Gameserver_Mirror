using System;
using System.Collections.Generic;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Communication.Filters
{
    /// <summary>
    ///     Provides authentication for requests for Swag Api.
    ///     Reads username and password from the header of the HttpRequest
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizationSwagAttribute : Attribute, IAuthorizationFilter
    {
        private const string UserCredentials = "username";
        private const string KeyCredentials = "password";

        public ILoginManager LoginManager => Application.Managers.LoginManager.GetInstance();

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tokens = context.HttpContext.Request.Headers;
            if (tokens.TryGetValue(UserCredentials, out var name) &&
                tokens.TryGetValue(KeyCredentials, out var pass))
            {
                var username = name.ToString();
                var password = pass.ToString();

                if (!LoginManager.CheckLoginStatus(username, password))
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }

    }
}