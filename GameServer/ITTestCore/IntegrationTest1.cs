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
        #region Variables

        private static TestServer _server;
        private static HttpClient _client;


        private IUserController fakeUserController;
        private ILobbyController fakeLobbyController;
        private ILobby fakeLobby;

        private readonly IUser _pers = Substitute.For<IUser>();

        private readonly IUser _pers2 = Substitute.For<IUser>();

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            // setup testserver
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupIntegrationTest1>());
            _server.Host.Start();
            _client = _server.CreateClient();

            //creating domain objects should these be fakes ´???

            ILobby testLobby = Substitute.For<ILobby>();
            testLobby.Id = "DenseLobby";
            testLobby.AdminUserName = "Maximillian";

            _pers2.Username = "PatrickPer";
            _pers2.GivenName = "Patrick";
            _pers2.LastName = "Per";
            _pers2.Password = "somethingU1";
            _pers2.Email = "123@123.com";

            _pers.Username = "Maximillian";
            _pers.GivenName = "Max";
            _pers.LastName = "Imilian";
            _pers.Password = "123456789";
            _pers.Email = "123@123.com";


            //Fakes Setup
            StartupIntegrationTest1.FakeLoginManager.CheckLoginStatus(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            StartupIntegrationTest1.FakeLobbyController.CreateLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(testLobby);
            StartupIntegrationTest1.FakeLobbyController.JoinLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(testLobby);
            StartupIntegrationTest1.FakeLobbyController.LeaveLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            fakeUserController = StartupIntegrationTest1.FakeUserController;
            fakeLobbyController = StartupIntegrationTest1.FakeLobbyController;
        }

        #endregion

        //*************************Test Communication Layer***************************

        #region LoginUser

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LoginUser()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            fakeUserController.Received().GetUser("Maximillian", "123456789");

        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LoginUser_LoginUserReturnsNull()
        {
            //arrange
            User nullUser = null;
            _client.DefaultRequestHeaders.Add("username", "NotMaximillian");
            _client.DefaultRequestHeaders.Add("password", "Not123456789");
            fakeUserController.GetUser("NotMaximillian", "Not123456789").Returns(nullUser);

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));

        }

        #endregion

        #region CreateUser

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateUser()
        {
            //assert
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
            fakeUserController.CreateUser(Arg.Any<IUser>()).Returns(_pers);

            //Concrete user is used, cuz serialization dont work on fakes.
            var tmp = new User();
            tmp.Username = _pers.Username;
            tmp.Email = _pers.Email;
            tmp.GivenName = _pers.GivenName;
            tmp.LastName = _pers.LastName;
            tmp.Password = _pers.Password;

            var stringContent = new StringContent(JsonConvert.SerializeObject(tmp), Encoding.UTF8, "application/json");

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            fakeUserController.Received().CreateUser(Arg.Any<IUser>());
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateUser_CreateUserReturnsNull()
        {
            //assert
            User nullUser = null;
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
            fakeUserController.CreateUser(Arg.Any<IUser>()).Returns(nullUser);

            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

        #region GetLobby

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

        #endregion

        #region CreateLobby

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
        public async Task IntegrationTest1_GameServer_CommunicationLayer_CreateLobby_CreateLobbyReturnsNull()
        {
            //arrange
            Lobby nullLobby = null;
            _client.DefaultRequestHeaders.Add("username", "Maximillian");
            _client.DefaultRequestHeaders.Add("password", "123456789");
            fakeLobbyController.CreateLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(nullLobby);

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId","DenseLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

        #region JoinLobby

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
        public async Task IntegrationTest1_GameServer_CommunicationLayer_JoinLobby_JoinLobbyReturnsNull()
        {
            //arrange
            Lobby nullLobby = null;
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            fakeLobbyController.JoinLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(nullLobby);


            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

        #region LeaveLobby

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

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_LeaveLobby_LeaveLobbyReturnsNull()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            fakeLobbyController.LeaveLobbyAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            var joinLobbyParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby"}
            };
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", joinLobbyParameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            //act
            HttpResponseMessage response = await _client.SendAsync(joinLobbyRequest);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

    }

  

}
