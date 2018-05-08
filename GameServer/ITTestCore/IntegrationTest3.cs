using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Application.Controllers;
using Application.Interfaces;
using Application.Managers;
using Communication.Formatters;
using Domain.DbAccess;
using Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using Persistance.Interfaces;
using User = Domain.Models.User;

namespace IT_Core
{
    #region StartupIntegrationTest3
    public class StartupIntegrationTest3
    {
        public static IDbContext FakeDbContext = Substitute.For<IDbContext>();

        public StartupIntegrationTest3(IConfiguration configuration)
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
            
            services.AddTransient<IUserRepository, UserRepository>(u =>
                new UserRepository(FakeDbContext, "UserCollection"));

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

    #region Misc

    public class HelperMethods
    {
        public static DocumentClientException CreateDocumentClientExceptionForTesting(
            Error error, HttpStatusCode httpStatusCode)
        {
            var type = typeof(DocumentClientException);

            // we are using the overload with 3 parameters (error, responseheaders, statuscode)
            // use any one appropriate for you.

            var documentClientExceptionInstance = type.Assembly.CreateInstance(type.FullName,
                false, BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { error, (HttpResponseHeaders)null, httpStatusCode }, null, null);

            return (DocumentClientException)documentClientExceptionInstance;
        }
    }
    #endregion

    [TestFixture]
    public class IntegrationTest3
    {
        private static TestServer _server;
        private static HttpClient _client;

        private IDbContext _fakeDbContext;
        private IDocumentClient _fakeDocumentClient;

        private IUser _testUserOne = new User()
        {
            Username = "GenericUsername",
            GivenName = "Generic",
            LastName  = "LastGeneric",
            Email = "generic@user.com",
            Password = "123456789"
        };

        private IUser _testUserTwo = new User()
        {
            Username = "GeneUsername",
            GivenName = "Gene",
            LastName = "LaGeneric",
            Email = "gene@user.com",
            Password = "987654321"
        };

        [SetUp]
        public void SetUp()
        {
            // setup testserver
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupIntegrationTest3>());
            _server.Host.Start();
            _client = _server.CreateClient();



            //Fakes Setup
            _fakeDbContext = StartupIntegrationTest3.FakeDbContext;
            _fakeDbContext.DocumentClient = Substitute.For<IDocumentClient>();
        }

        [TearDown]
        public void TearDown()
        {
            _fakeDocumentClient = null;
            _client = null;
            _server = null;
            _fakeDbContext = null;
        }

        #region UserRepositoryTests
        [Test]
        public async Task IntegrationTest3_DomainLayer_UserRepository_GetItemAsync_ReadDocumentAsyncIsCalled_ResponseNotFound()
        {
            //arrange
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Code = "404",
                Message = "not found"
            };

            var testException1 = HelperMethods.CreateDocumentClientExceptionForTesting(error,
                HttpStatusCode.NotFound);
            _fakeDbContext.DocumentClient.Returns(x => throw testException1);


            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            
            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_UserRepository_GetItemAsync_ReadDocumentAsyncIsCalled_ResponseOk()
        {
            //arrange
            
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");

            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());

            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var documentClientResponse = new ResourceResponse<Document>(document);
                   
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));
   

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_UserRepository_CreateItemAsync_CreateDocumentAsyncIsCalled_ResponseNotFound()
        {
            //arrange
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Code = "409",
                Message = "bad request"
            };

            var testException = HelperMethods.CreateDocumentClientExceptionForTesting(error,
                HttpStatusCode.Conflict);

            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            _fakeDbContext.DocumentClient.Returns(x => throw testException);

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_UserRepository_CreateItemAsync_CreateDocumentAsyncIsCalled_ResponseCreated()
        {
            //arrange

            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());

            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var documentClientResponse = new ResourceResponse<Document>(document);
            
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));

            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        #endregion

        #region CreateLobbyTests

        [Test]
        public async Task IntegrationTest3_DomainLayer_CreateLobbyWithRealLobby_ResponseBadRequest()
        {
            //arrange

            //SetUpdFakes
            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());
            var documentClientResponse = new ResourceResponse<Document>(document);
           
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));

            //Create User
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse = await _client.PostAsync("api/User", stringContent);
            
            //Create empty Lobby
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response = await _client.SendAsync(request);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
        
        [Test]
        public async Task IntegrationTest3_DomainLayer_CreateLobbyWithRealLobby_ResponseCreated()
        {
            //arrange

            //SetUpdFakes
            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());
            var documentClientResponse = new ResourceResponse<Document>(document);

            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));

            //Create User
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse = await _client.PostAsync("api/User", stringContent);

            //Create Lobby 1
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response1 = await _client.SendAsync(request);
            //assert
            Assert.That(response1.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_CreateLobbyWithRealLobby_ResponseUnAuthorized()
        {
            //arrange
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Code = "404",
                Message = "Not Found"
            };

            var testException = HelperMethods.CreateDocumentClientExceptionForTesting(error,
                HttpStatusCode.NotFound);

            //SetUpdFakes
            
            _fakeDbContext.DocumentClient.Returns(x => throw testException);


            //Create Lobby 1
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            

            //act
            HttpResponseMessage response1 = await _client.SendAsync(request);
            //assert
            Assert.That(response1.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }


        [Test]
        public async Task IntegrationTest3_DomainLayer_CreateIdenticalLobbyWithRealLobby_ResponseBadRequest()
        {
            //arrange

            //SetUpdFakes
            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());
            var documentClientResponse = new ResourceResponse<Document>(document);

            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));

            //Create User
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse = await _client.PostAsync("api/User", stringContent);

            //Create Lobby 1
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var requestUri1 = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request1 = new HttpRequestMessage(HttpMethod.Post, requestUri1);

            //Create Lobby 2 (Identical)

            var requestUri2 = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request2 = new HttpRequestMessage(HttpMethod.Post, requestUri2);

            //Log in
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            var loginResponse = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage response1 = await _client.SendAsync(request1);
            HttpResponseMessage response2 = await _client.SendAsync(request2);

            //assert
            Assert.That(response1.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(response2.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

        #region GetLobbyTests

        [Test]
        public async Task IntegrationTest3_DomainLayer_GetLobby_ResponseOk()
        {
            //arrange
            //SetUpdFakes
            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());
            var documentClientResponse = new ResourceResponse<Document>(document);

            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));

            //Create User
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse = await _client.PostAsync("api/User", stringContent);

            //Create Lobby 1
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            var loginResponse = await _client.GetAsync("api/User/Login");
            var createLobbyResponse = await _client.SendAsync(request);

            //act
            var response = await _client.GetAsync("api/Lobby/AwesomeTestLobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_GetLobby_ResponseNotFound()
        {
            //act
            var response = await _client.GetAsync("api/Lobby/DenseLobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_GetAllLobbies_ResponseOk()
        {
            //act
            var response = await _client.GetAsync("api/Lobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_CreateLobby_GetAllLobbies_ResponseOk()
        {
            //arrange
            //SetUpdFakes
            var document = new Document();
            JObject jsobObj = JObject.FromObject(_testUserOne);
            document.LoadFrom(jsobObj.CreateReader());
            var documentClientResponse = new ResourceResponse<Document>(document);

            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponse));
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponse));

            //Create User
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse = await _client.PostAsync("api/User", stringContent);

            //Create Lobby 1
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var requestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            //Log in
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            var loginResponse = await _client.GetAsync("api/User/Login");
            var createLobbyResponse = await _client.SendAsync(request);

            //act
            var response = await _client.GetAsync("api/Lobby");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        #endregion

        #region JoinLobbyTests

        [Test]
        public async Task IntegrationTest3_DomainLayer_JoinLobbyUser2_ResponseCreated()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);
            
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Log in User 2 and join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            var loginResponse2 = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);
            //assert
            Assert.That(joinLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_JoinLobbyUser1_ResponseBadRequest()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);


            //act
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);
            //assert
            Assert.That(joinLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_JoinLobbyUser2_ResponseUnauthorized()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Dont Log in User 2, but join lobby user 2
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);

            //act
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);
            //assert
            Assert.That(joinLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_JoinLobbyUser2WhileInLobby_ResponseBadRequest()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters1 = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };

            var parameters2 = new Dictionary<string, string>()
            {
                {"lobbyId", "AwesomeTestLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters1);
            var joinLobbyRequestUri1 = QueryHelpers.AddQueryString("api/Lobby/Join", parameters1);
            var joinLobbyRequestUri2 = QueryHelpers.AddQueryString("api/Lobby/Join", parameters2);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest1 = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri1);
            var joinLobbyRequest2 = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri2);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Log in User 2 and join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            var loginResponse2 = await _client.GetAsync("api/User/Login");

            //act
            HttpResponseMessage joinLobbyResponse1 = await _client.SendAsync(joinLobbyRequest1);
            HttpResponseMessage joinLobbyResponse2 = await _client.SendAsync(joinLobbyRequest2);
            //assert
            Assert.That(joinLobbyResponse2.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        #endregion

        #region LeaveLobbyTests

        [Test]
        public async Task IntegrationTest3_DomainLayer_LeaveLobbyUser2_ResponseRedirect()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);
            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Log in User 2 and join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            var loginResponse2 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);

            //act

            HttpResponseMessage leaveLobbyResponse = await _client.SendAsync(leaveLobbyRequest);

            //assert
            Assert.That(leaveLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Redirect));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_LeaveLobbyUser1_ResponseRedirect()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);
            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Log in User 2 and join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            var loginResponse2 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);

            //Log in User 1
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            loginResponse1 = await _client.GetAsync("api/User/Login");

            //act

            HttpResponseMessage leaveLobbyResponse = await _client.SendAsync(leaveLobbyRequest);

            //assert
            Assert.That(leaveLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Redirect));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_LeaveLobbyUser1NoUsersInLobby_ResponseRedirect()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);
            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);
            //act

            HttpResponseMessage leaveLobbyResponse = await _client.SendAsync(leaveLobbyRequest);

            //assert
            Assert.That(leaveLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Redirect));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_LeaveLobbyUser1_ReturnsBadRequest()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));
            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);


            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", parameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Log in User 2 and join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("username", _testUserTwo.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);
            var loginResponse2 = await _client.GetAsync("api/User/Login");

            //act

            HttpResponseMessage leaveLobbyResponse = await _client.SendAsync(leaveLobbyRequest);

            //assert
            Assert.That(leaveLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task IntegrationTest3_DomainLayer_LeaveLobbyUser2_UnAuthorized()
        {
            //arrange

            //SetUpdFakes
            var documentUser1 = new Document();
            JObject jsobObjUser1 = JObject.FromObject(_testUserOne);
            documentUser1.LoadFrom(jsobObjUser1.CreateReader());
            var documentClientResponseUser1 = new ResourceResponse<Document>(documentUser1);

            var documentUser2 = new Document();
            JObject jsobObjUser2 = JObject.FromObject(_testUserTwo);
            documentUser2.LoadFrom(jsobObjUser2.CreateReader());
            var documentClientResponseUser2 = new ResourceResponse<Document>(documentUser2);

            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser1));

            //Create User 1 & 2
            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser1));

            var stringContentUser1 = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            var createUserResponse1 = await _client.PostAsync("api/User", stringContentUser1);

            _fakeDbContext.DocumentClient.CreateDocumentAsync(Arg.Any<Uri>(), Arg.Any<IUser>()).Returns(Task.FromResult(documentClientResponseUser2));
            var stringContentUser2 = new StringContent(JsonConvert.SerializeObject(_testUserTwo), Encoding.UTF8, "application/json");
            var createUserResponse2 = await _client.PostAsync("api/User", stringContentUser2);

            //Create & join Lobby 1 requests
            var nullParameters = new Dictionary<string, string>()
            {
                {"lobbyId", "s" }
            };
            var parameters = new Dictionary<string, string>()
            {
                {"lobbyId", "DenseLobby" }
            };
            var createLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Create", parameters);
            var joinLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Join", parameters);
            var leaveLobbyRequestUri = QueryHelpers.AddQueryString("api/Lobby/Leave", nullParameters);

            var createLobbyRequest = new HttpRequestMessage(HttpMethod.Post, createLobbyRequestUri);
            var joinLobbyRequest = new HttpRequestMessage(HttpMethod.Post, joinLobbyRequestUri);
            var leaveLobbyRequest = new HttpRequestMessage(HttpMethod.Post, leaveLobbyRequestUri);

            //Log in User 1 and create lobby
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var loginResponse1 = await _client.GetAsync("api/User/Login");
            HttpResponseMessage createLobbyResponse = await _client.SendAsync(createLobbyRequest);

            //setup fake for login 2
            _fakeDbContext.DocumentClient.ReadDocumentAsync(Arg.Any<Uri>()).Returns(Task.FromResult(documentClientResponseUser2));

            //Dont Log in User 2 and then join lobby
            _client.DefaultRequestHeaders.Remove("username");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Add("password", _testUserTwo.Password);

            HttpResponseMessage joinLobbyResponse = await _client.SendAsync(joinLobbyRequest);

            //act

            HttpResponseMessage leaveLobbyResponse = await _client.SendAsync(leaveLobbyRequest);

            //assert
            Assert.That(leaveLobbyResponse.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Unauthorized));
        }

        #endregion
    }
}
