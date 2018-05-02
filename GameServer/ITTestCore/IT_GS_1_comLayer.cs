using System.Net.Http;
using Application.Interfaces;
using Communication;
using Communication.RESTControllers;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace IT_Core
{
    
    [TestFixture] //<--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_Test1
    {
        private static string _postmanUrl = "https://b594b385-5e55-41b2-8865-926a5168a42b.mock.pstmn.io";
        private static TestServer _server;
        private static HttpClient _client;

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Username";
        string pass = "Pass";

        private IUserController fakeProvider;
        private User pers = new User();
        private UserController _uut;





        [SetUp]
        public void Setup()
        {
            string user = "Maximilian";
            string givename = "Max";
            string last = "Imilian";
            string pass = "123456789";
            string email = "123@123.com";

            pers.Username = user;
            pers.GivenName = givename;
            pers.LastName = last;
            pers.Password = pass;
            pers.Email = email;

            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<StartupIntegrationTest1>());
            _client = _server.CreateClient();
            fakeProvider = StartupIntegrationTest1.FakeProvider;
        }


        //*************************Test Communication Layer***************************

        //Test if we sent username and password to Usercontroller in communicationlayer, it gets received in usercontroler in applicationLayer
        [Test]
        public void IT_1_GS_LoginUser()
        {
            
            var client = new RestClient(_postmanUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Postman-Token", "c2217970-30d1-4ce2-849d-57097486dcb9");
            request.AddHeader("Cache-Control", "no-cache");
            IRestResponse response = client.Execute(request);

            fakeProvider.GetUser("Maximillian", "123456789").Received(1);
            

        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public void IT_1_GS_CreateUser()
        {
            var client = new RestClient("http://localhost:50244/api/User");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Postman-Token", "43d0097f-6144-41a2-9739-c4daba54ed3c");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("password", "123456789");
            request.AddHeader("username", "Maximillian");
            request.AddParameter("undefined", "{\n\t\"id\" : \"Maximilian\",\n\t\"GivenName\" : \"Max\",\n\t\"LastName\" : \"Imilian\",\n\t\"Email\" : \"123@123.com\",\n\t\"Password\" : \"123456789\"\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            fakeProvider.CreateUser(pers).Received(1);

        }
        


    }

    public class StartupIntegrationTest1 : Startup
    {
        public static IUserController FakeProvider = Substitute.For<IUserController>();

        public StartupIntegrationTest1(IConfiguration configuration) : base(configuration)
        {

        }

        public void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            
            services.AddTransient<IUserController>(provider => FakeProvider);
        }
    }

}