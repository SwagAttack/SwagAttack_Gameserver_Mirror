using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Communication.Authentication
{

    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ApiValidator
    {
        private readonly RequestDelegate _next;
        ILoginManager _loginManager { get; set; }

        public ApiValidator(RequestDelegate next, ILoginManager loginManager)
        {
            _next = next;
            _loginManager = loginManager;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                if (httpContext.Request.Path.StartsWithSegments("/api/User/") && (httpContext.Request.Method == "GET" || httpContext.Request.Method == "PUT"))
                {
                    
                }
                else if (httpContext.Request.Path.StartsWithSegments("/api"))
                {
                    var queryString = httpContext.Request.Query;
                    StringValues keyvalue;

                    queryString.TryGetValue("password", out keyvalue);
                    queryString.TryGetValue("username", out keyvalue);

                   

                    if (httpContext.Request.Method != "GET")
                    {
                        httpContext.Response.StatusCode = 405; //Method Not Allowed               
                        await httpContext.Response.WriteAsync("Method Not Allowed");
                        return;
                    }

                    if (keyvalue.Count != 2)
                    {
                        httpContext.Response.StatusCode = 400; //Bad Request                
                        await httpContext.Response.WriteAsync("API Key is missing");
                        return;
                    }
                    else
                    {

                        if (!_loginManager.CheckLoginStatus(keyvalue[1])) 
                        {
                            httpContext.Response.StatusCode = 401; //UnAuthorized
                            await httpContext.Response.WriteAsync("Aunauthorized Request");
                            return;
                        }
                    }

                }
                await _next.Invoke(httpContext);

            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiValidator>();
        }
    }
}