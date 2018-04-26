using Application.Controllers;
using Application.Interfaces;
using Application.Managers;
using Communication.Formatters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistance;
using Persistance.UnitOfWork;

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
                //options.Filters.Add(typeof(ValidateModelStateAttribute));
                //options.Filters.Add(new RequireHttpsAttribute());
                options.InputFormatters.Insert(0, new JsonInputFormatter());
            });

            services.AddTransient<IUnitOfWork>(u => new UnitOfWork(new DbContext()));
            services.AddSingleton(l => LoginManager.GetInstance());
            services.AddSingleton<ILobbyController, LobbyController>();
            services.AddTransient<IUserController, UserController>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            //var options = new RewriteOptions().AddRedirectToHttps();
            //app.UseRewriter(options);

            app.UseMvc();
        }
    }
}