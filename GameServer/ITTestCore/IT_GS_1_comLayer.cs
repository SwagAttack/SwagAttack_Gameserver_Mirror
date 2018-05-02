using System.Net.Http;
using Communication;
using Communication.RESTControllers;
using Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using RestSharp;

namespace IT_Core
{
    
    //[TestFixture] <--- Er blevet udkommenteret, da en textFixture uden tests får Travis til at fejle
    public class IT_Test
    {
        private static string _postmanUrl = "https://b594b385-5e55-41b2-8865-926a5168a42b.mock.pstmn.io";
        private static TestServer _server;
        private static HttpClient _client;

        //UserController.LoginDto LoginInfo = new UserController.LoginDto();
        string user = "Username";
        string pass = "Pass";

        private User pers = new User();
        private UserController _uut;





        [SetUp]
        public void Setup()
        {
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

            // Arrange
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _client = _server.CreateClient();
            
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

            var tmp = response.Headers.Count;
            //UC_mock.Verify(x => x.GetUser(pers.Username, pers.Password));

        }

        //Test if Createuser with pers as userobj, gets received in AppLayer CreateUser
        [Test]
        public void IT_2_GS_CreateUser()
        {
            _uut.CreateUser(pers);
            //UC_mock.Verify(x => x.CreateUser(pers));

        }

        //Test if Update with pers as userobj, gets received in AppLayer UpdateUser
        [Test]
        public void IT_3_GS_UpdateUser()
        {
            _uut.UpdateUser(pers.Username, pers);
            //UC_mock.Verify(x => x.UpdateUser(pers.Username, pers));

        }


    }
    

}