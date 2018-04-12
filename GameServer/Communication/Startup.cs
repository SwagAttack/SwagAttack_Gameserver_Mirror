using System;
using System.Text;
using Application.Controllers;
using Application.Interfaces;
using DBInterface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DBInterface.UnitOfWork;
using Communication.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.IdentityModel.Tokens;
using Models.User;

namespace Communication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateAttribute));
                //options.Filters.Add(new RequireHttpsAttribute());
            });

            //services.AddAuthentication(options => {
            //        options.DefaultAuthenticateScheme = "JwtBearer";
            //        options.DefaultChallengeScheme = "JwtBearer";
            //    })
            //    .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            //    {
            //        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuerSigningKey = true,
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("My own test key yeah")),

            //            ValidateAudience = false,
            //            ValidateIssuer = false,
                        

            //            ValidateLifetime = true, //validate the expiration and not before values in the token

            //            ClockSkew = TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
            //        };
            //    });

            services.AddTransient<IUnitOfWork>(u => new UnitOfWork(new DbContext()));
            services.AddTransient<IUserController, UserController>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //var options = new RewriteOptions().AddRedirectToHttps();

            //app.UseRewriter(options);

            // app.UseAuthentication();

            app.UseMvc();
        }
    }
}
