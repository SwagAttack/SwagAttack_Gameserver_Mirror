using System;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

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

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var loginManager = context.HttpContext.RequestServices.GetService<ILoginManager>();

            var tokens = context.HttpContext.Request.Headers;

            var confirmed = false;

            if (tokens.TryGetValue(UserCredentials, out var name) &&
                tokens.TryGetValue(KeyCredentials, out var pass))
            {
                var username = name.ToString();
                var password = pass.ToString();

                confirmed = loginManager.CheckLoginStatus(username, password);
            }

            if(!confirmed)
                context.Result = new UnauthorizedResult();
        }

    }
}