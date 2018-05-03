using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
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
    #region IntegrationTest2Startup
    public class StartupIntegrationTest2
    {
        public static IUserRepository FakeUserRepository = Substitute.For<IUserRepository>();

        public StartupIntegrationTest2(IConfiguration configuration)
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
            //services.AddSingleton<IDbContext>(provider => FakeDbContext);


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
    #endregion

    [TestFixture]
    public class IntegrationTest2
    {
        private static TestServer _server;
        private static HttpClient _client;


        private IUserRepository _fakeUserRepository;
        private IDbContext _fakeDbContext;

        
        private readonly User _pers = new User()
        {
            Username = "PatrickPer",
            GivenName = "Patrick",
            LastName = "Per",
            Password = "somethingU1",
            Email = "123@123.com"
        };

        private readonly User _pers2 = new User()
        {
            Username = "DamnTestNames",
            GivenName = "Damn",
            LastName = "Names",
            Password = "TestTestTest",
            Email = "123@123.com"
        };

        private readonly Lobby _testLooby = new Lobby("DamnTestNames")
        {
            Id = "DamnTestLobby"
        };


        [SetUp]
        public void Setup()
        {
            // setup testserver
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupIntegrationTest2>());
            _server.Host.Start();
            _client = _server.CreateClient();
            
            //Fakes Setup
            _fakeUserRepository = StartupIntegrationTest2.FakeUserRepository;
            //_fakeDbContext = StartupIntegrationTest2.FakeDbContext;

        }


        //*************************Test App Layer***************************
        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_GetUser_GetItemAsyncIsCalled()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            await _fakeUserRepository.Received().GetItemAsync(_pers.Username);

        }

        [Test,]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_GetUserReturnsNull_NotFoundResponse()
        {
            //arrange
            User nullUser = null;
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(Arg.Any<string>()).Returns(nullUser);
            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_GetUserReturnsUser_ResponseOK()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

        }
        
        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateUser_CreateItemAsyncIsCalled()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            await _fakeUserRepository.Received().CreateItemAsync(Arg.Is<IUser>(x => x.Username == _pers.Username));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateUser_CreateItemAsyncReturnsNull()
        {
            //arrange
            User nullUser = null;
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            var stringContent = new StringContent(JsonConvert.SerializeObject(_pers), Encoding.UTF8, "application/json");
            _fakeUserRepository.CreateItemAsync(Arg.Any<User>()).Returns(nullUser);

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateUser_ResponseCreated()
        {
            //arrange
            //Concrete user is used, cuz serialization dont work on fakes.
            var tmp = new User();
            tmp.Username = _pers2.Username;
            tmp.Email = _pers2.Email;
            tmp.GivenName = _pers2.GivenName;
            tmp.LastName = _pers2.LastName;
            tmp.Password = _pers2.Password;

            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            _fakeUserRepository.CreateItemAsync(Arg.Any<User>()).Returns(_pers2);

            var stringContent = new StringContent(JsonConvert.SerializeObject(tmp), Encoding.UTF8, "application/json");

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetAllLobbies()
        {
            //act
            var response = await _client.GetAsync("api/Lobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetLobbyReturnsOk()
        {
            //arrange
            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var CreateLobbyrequest = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //create a lobby
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(CreateLobbyrequest);


            //act
            var response = await _client.GetAsync("api/Lobby/AwesomeTestLobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public async Task IntegrationTest1_GameServer_CommunicationLayer_GetLobbyReturnsNotFound()
        {
            //act
            var response = await _client.GetAsync("api/Lobby/DenseLobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateLobby_ResponseCreated()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateLobby_ResponseBadRequest()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_CreateTwoIdenticalLobbies_ResponseBadRequest()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request1 = new HttpRequestMessage(HttpMethod.Post, requestUri);
            var request2 = new HttpRequestMessage(HttpMethod.Post, requestUri);
            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response1 = await _client.SendAsync(request1);
            HttpResponseMessage response2 = await _client.SendAsync(request2);

            //assert
            Assert.That(response2.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_JoinLobby_ResponseCreated()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            _fakeUserRepository.GetItemAsync(_pers2.Username).Returns(_pers2);

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);

            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);


            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //Create Lobby
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //log in with new user 
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);
            
            loginResponse = await _client.GetAsync("api/User/Login");

            //JoinLobby
            HttpResponseMessage response = await _client.SendAsync(joinLobbyRequest);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_JoinLobby_ResponseBadRequest()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            _fakeUserRepository.GetItemAsync(_pers2.Username).Returns(_pers2);

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };

            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);


            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");
            
            //log in with new user 
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);

            loginResponse = await _client.GetAsync("api/User/Login");

            //JoinLobby
            HttpResponseMessage response = await _client.SendAsync(joinLobbyRequest);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_LeaveLobby_ResponseRedirect()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            _fakeUserRepository.GetItemAsync(_pers2.Username).Returns(_pers2);

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);

            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //Create Lobby
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //log in with new user 
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);

            loginResponse = await _client.GetAsync("api/User/Login");

            //JoinLobby
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);

            //Act
            HttpResponseMessage response = await _client.SendAsync(leaveLobbyRequest);


            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Redirect));

        }

        [Test]
        public async Task IntegrationTest2_GameServer_ApplicationLayer_LeaveLobby_ResponseBadRequest()
        {
            //arrange

            _client.DefaultRequestHeaders.Add("username", _pers.Username);
            _client.DefaultRequestHeaders.Add("password", _pers.Password);
            _fakeUserRepository.GetItemAsync(_pers.Username).Returns(_pers);
            _fakeUserRepository.GetItemAsync(_pers2.Username).Returns(_pers2);

            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in
            var loginResponse = await _client.GetAsync("api/User/Login");

            //Create Lobby
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //log in with new user 
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _pers2.Username);
            _client.DefaultRequestHeaders.Add("password", _pers2.Password);

            loginResponse = await _client.GetAsync("api/User/Login");

            //JoinLobby

            //Act
            HttpResponseMessage response = await _client.SendAsync(leaveLobbyRequest);


            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));

        }

    }

}