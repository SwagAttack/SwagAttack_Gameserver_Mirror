using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Application.Controllers;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Communication.Formatters;
using Communication.RESTControllers;
using Domain.DbAccess;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Persistance;
using Persistance.Interfaces;
using Persistance.Setup;
using LobbyController = Application.Controllers.LobbyController;
using UserController = Application.Controllers.UserController;

namespace IT_Core
{
    public class IntegrationTest2Startup
    {
        public IDbContext FakeDbContext = Substitute.For<IDbContext>();
        public IUserRepository FakeUserRepository = Substitute.For<IUserRepository>();

        public IntegrationTest2Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.InputFormatters.Insert(0, new JsonInputFormatter());
            });

            // Database fake
            services.AddSingleton<IDbContext>(provider => FakeDbContext);


            // Usercollection fake
            services.AddTransient<IUserRepository>(provider => FakeUserRepository);

            services.AddSingleton(l => LoginManager.GetInstance());
            services.AddSingleton<ILobbyController, LobbyController>();
            services.AddTransient<IUserController, UserController>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseMvc();
        }
    }
    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IntegrationTest2
    {
/*
        string user = "Username";
        string pass = "Pass";

        private User pers = new User();
        private Mock<IUnitOfWork> OUW = new Mock<IUnitOfWork>();
        private IUnitOfWork pOUW;
        private ILoginManager pLM;
        private Mock<ILoginManager> LM = new Mock<ILoginManager>();
        private Mock<UnitOfWork> OUWM = new Mock<UnitOfWork>();
        private Application.Controllers.UserController _uut;
*/

        [SetUp]
        public void Setup()
        {
            /*_uut = new UserController(pOUW,pLM);
            string user = "UsernameIT";
            string givename = "GivennameIT";
            string last = "LastNameIT";
            string pass = "PasswordIT";
            string email = "dummy@dummy.dkIT";

            pers.Username = user;
            pers.GivenName = givename;
            pers.LastName = last;
            pers.Password = pass;
            pers.Email = email;
*/

        }


        //*************************Test App Layer***************************

        //Test if we send usercontroler in applicationLayer gets received in persistance Layer


        //Test if Createuser with pers as userobj, gets received in persistance Layer




    }

}