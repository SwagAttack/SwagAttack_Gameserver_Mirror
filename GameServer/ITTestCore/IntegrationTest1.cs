using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Managers;
using Communication;
using Communication.Formatters;
using Communication.RESTControllers;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace IT_Core
{
    
    [TestFixture] //<--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IntegrationTest1
    {
        private static string _postmanUrl = "https://b594b385-5e55-41b2-8865-926a5168a42b.mock.pstmn.io";
        private static TestServer _server;
        private static HttpClient _client;

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Maximillian";
        string pass = "123456789";

        private IUserController fakeUserController;
        private ILobbyController fakeLobbyController;
        private ILobby fakeLobby;

        private readonly User _pers = new User();

        private readonly User _pers2 = new User()
        {
            Username = "PatrickPer",
            GivenName = "Patrick",
            LastName = "Per",
            Password = "somethingU1",
            Email = "123@123.com"
        };
        
        [SetUp]
        public void Setup()
        {
            Lobby testLobby = new Lobby("Maximillian");
            testLobby.Id = "DenseLobby";
            testLobby.AdminUserName = "Maximillian";

            Lobby testLobby2 = testLobby;
            testLobby2.AddUser(_pers2.Username);


            _pers.Username = user;
            _pers.GivenName = "Max";
            _pers.LastName = "Imilian";
            _pers.Password = pass;
            _pers.Email = "123@123.com";

            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseUrls("http://localhost:50244/")
                .UseStartup<StartupIntegrationTest1>());
            _server.Host.Start();

            _client = _server.CreateClient();
            StartupIntegrationTest1.FakeLoginManager.CheckLoginStatus(Arg.Any<string>(),Arg.Any<string>()).Returns(true);
            StartupIntegrationTest1.FakeLobbyController.CreateLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(testLobby);
            StartupIntegrationTest1.FakeLobbyController.JoinLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(testLobby2);
            StartupIntegrationTest1.FakeLobbyController.LeaveLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            fakeUserController = StartupIntegrationTest1.FakeUserController;
            fakeLobbyController = StartupIntegrationTest1.FakeLobbyController;
        }


        //*************************Test Communication Layer***************************
        //These tests are made by using PostMan to test the api.
        //Test if we sent username and password to Usercontroller in communicationlayer, it gets received in usercontroler in applicationLayer
        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LoginUser()
        {

            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");
            var responseCreateUser = await _client.PostAsync("api/User", stringContent);
        
            var response = await _client.GetAsync("api/User/Login");

            var tmp = response;

            fakeUserController.Received(1).GetUser("Maximillian","123456789");
            
        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateUser()
        {
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");


            var response = await _client.PostAsync("api/User", stringContent);

            
            fakeUserController.Received(1).CreateUser(Arg.Is<IUser>(x => x.Username == "Maximillian"));
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetAllLobbies()
        {
            var response = await _client.GetAsync("api/Lobby");

            await fakeLobbyController.Received(1).GetAllLobbiesAsync();
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetLobby()
        {
            var response = await _client.GetAsync("api/Lobby/DenseLobby");

            await fakeLobbyController.Received(1).GetLobbyByIdAsync("DenseLobby");
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateLobby()
        {
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");
            var responseCreateUser = await _client.PostAsync("api/User", stringContent);

            var loginResponse = await _client.GetAsync("api/User/Login");

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId","DenseLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);



            HttpResponseMessage response = await _client.SendAsync(request);



            await fakeLobbyController.Received(1).CreateLobbyAsync("DenseLobby", "Maximillian");
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_JoinLobby()
        {
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
            var stringContent1 =
                new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");
            var stringContent2 =
                new StringContent(JsonConvert.SerializeObject(_pers2), Encoding.UTF8, "application/json");

            //Create 2 users and login with first

            var responseCreateUser1 = await _client.PostAsync("api/User", stringContent1);
            var responseCreateUser2 = await _client.PostAsync("api/User", stringContent2);
            var loginResponseUser1 = await _client.GetAsync("api/User/Login");

            //Create Lobby

            var createLobbyParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", createLobbyParameters);
            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);

            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //Log in with second user
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            var loginResponseUser2 = await _client.GetAsync("api/User/Login");

            //Join Lobby

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            
            HttpResponseMessage response = await _client.SendAsync(request);

            await fakeLobbyController.Received(1).JoinLobbyAsync("DenseLobby", _pers2.Username);
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LeaveLobby()
        {
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
            var stringContent1 =
                new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");
            var stringContent2 =
                new StringContent(JsonConvert.SerializeObject(_pers2), Encoding.UTF8, "application/json");

            //Create 2 users and login with first

            var responseCreateUser1 = await _client.PostAsync("api/User", stringContent1);
            var responseCreateUser2 = await _client.PostAsync("api/User", stringContent2);
            var loginResponseUser1 = await _client.GetAsync("api/User/Login");

            //Create Lobby

            var createLobbyParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", createLobbyParameters);
            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);

            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //Log in with second user
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            var loginResponseUser2 = await _client.GetAsync("api/User/Login");

            //Join Lobby

            var joinLobbyParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", joinLobbyParameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            HttpResponseMessage response = await _client.SendAsync(joinLobbyRequest);

            //Leave Lobby



            await fakeLobbyController.Received(1).LeaveLobbyAsync("DenseLobby", _pers2.Username);
        }


    }

    public class StartupIntegrationTest1
    {
        public static IUserController FakeUserController = Substitute.For<IUserController>();
        public static ILobbyController FakeLobbyController = Substitute.For<ILobbyController>();
        public static ILoginManager FakeLoginManager = Substitute.For<ILoginManager>();

        public StartupIntegrationTest1(IConfiguration configuration)
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

            services.AddSingleton<ILoginManager>(provider => FakeLoginManager);
            services.AddTransient<IUserController>(provider => FakeUserController);
            services.AddTransient<ILobbyController>(provider => FakeLobbyController);
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
