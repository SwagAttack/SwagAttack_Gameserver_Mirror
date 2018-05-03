using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Communication.Formatters;
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

namespace IT_Core
{
    #region StartupIntegrationTest1
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

            app.UseMvc();
        }
    }
    #endregion

    [TestFixture] 
    public class IntegrationTest1
    {
        private static TestServer _server;
        private static HttpClient _client;

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
        //Test if we sent username and password to Usercontroller in communicationlayer, it gets received in usercontroler in applicationLayer
        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LoginUser()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
        
            //act
            var response = await _client.GetAsync("api/User/Login");
            
            //assert
            fakeUserController.Received().GetUser("Maximillian","123456789");
            
        }


        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateUser()
        {
            //assert
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            fakeUserController.Received().CreateUser(Arg.Is<IUser>(x => x.Username == "Maximillian"));
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetAllLobbies()
        {
            //act
            var response = await _client.GetAsync("api/Lobby");

            //assert
            await fakeLobbyController.Received().GetAllLobbiesAsync();
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetLobby()
        {
            //act
            var response = await _client.GetAsync("api/Lobby/DenseLobby");

            //assert
            await fakeLobbyController.Received(1).GetLobbyByIdAsync("DenseLobby");
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateLobby()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId","DenseLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            
            //act
            HttpResponseMessage response = await _client.SendAsync(request);
            
            //assert
            await fakeLobbyController.Received().CreateLobbyAsync("DenseLobby", "Maximillian");
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_JoinLobby()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            
            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            await fakeLobbyController.Received().JoinLobbyAsync("DenseLobby", _pers2.Username);
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LeaveLobby()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
          
            var joinLobbyParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", joinLobbyParameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            //act
            HttpResponseMessage response = await _client.SendAsync(joinLobbyRequest);

            //assert
            await fakeLobbyController.Received().LeaveLobbyAsync("DenseLobby", _pers.Username);
        }


    }

  

}
