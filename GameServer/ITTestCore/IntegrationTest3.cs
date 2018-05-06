using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
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

    [TestFixture, NonParallelizable]
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
            _fakeDbContext = null;
        }

        [Test, NonParallelizable]
        public async Task IntegrationTask3_DomainLayer_UserRepository_GetItemAsync_ReadDocumentAsyncIsCalled_ResponseNotFound()
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

        [Test, NonParallelizable]
        public async Task IntegrationTask3_DomainLayer_UserRepository_GetItemAsync_ReadDocumentAsyncIsCalled_ResponseOk()
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
        public async Task IntegrationTask3_DomainLayer_UserRepository_CreateItemAsync_CreateDocumentAsyncIsCalled_ResponseNotFound()
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
        public async Task IntegrationTask3_DomainLayer_UserRepository_CreateItemAsync_CreateDocumentAsyncIsCalled_ResponseCreated()
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

    }
}
