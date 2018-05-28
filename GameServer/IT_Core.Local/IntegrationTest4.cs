using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Communication;
using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using NUnit.Framework;
using User = Domain.Models.User;

namespace IT_Core
{
    #region misc

    public static class Misc
    {
        //This is heavily inspired by https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static IUser GetNewTestUser()
        {
            int length = random.Next(8, 16);

            return new User()
            {
                Username = RandomString(length),
                GivenName = "Gene",
                LastName = "LaGeneric",
                Email = "gene@user.com",
                Password = "987654321"
            };
        }
    }

    #endregion

    /// <summary>
    /// Please make sure that Azzure Cosmos Db Emulator is running when executing tests.
    /// These tests can only be run locally, thus they need to be in a seperate project
    /// than the rest of the integration tests in order to be excluded on CI build-servers.
    /// </summary>
    [TestFixture]
    public class IntegrationTest4
    {
        private static TestServer _server;
        private static HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            // setup testserver
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>());
            _server.Host.Start();
            _client = _server.CreateClient();

        }

        [TearDown]
        public void TearDown()
        {
            _client = null;
            _server = null;
        }

        #region CreateUserTests
        [Test]
        public async Task IntegrationTest4_CreateUser_ResponseOk()
        {
            //arrange
            var _testUserOne = Misc.GetNewTestUser();

            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);



            //act
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        }

        [Test]
        public async Task IntegrationTest4_CreateUser_ResponseBadRequest()
        {
            //arrange
            var _testUserOne = Misc.GetNewTestUser();

            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");
            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);

            //create user
            await _client.PostAsync("api/User", stringContent);

            //act (create identical user == bad request)
            var response = await _client.PostAsync("api/User", stringContent);

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
        #endregion

        #region GetUserTests
        [Test]
        public async Task IntegrationTest4_GetUser_ResponseNotFound()
        {
            //arrange
            var _testUserOne = Misc.GetNewTestUser();

            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);



            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        }

        [Test]
        public async Task IntegrationTest4_GetUser_ResponseOk()
        {
            //arrange
            var _testUserOne = Misc.GetNewTestUser();

            _client.DefaultRequestHeaders.Add("username", _testUserOne.Username);
            _client.DefaultRequestHeaders.Add("password", _testUserOne.Password);
            var stringContent = new StringContent(JsonConvert.SerializeObject(_testUserOne), Encoding.UTF8, "application/json");

            //Create User
            await _client.PostAsync("api/User", stringContent);

            //act
            var response = await _client.GetAsync("api/User/Login");

            //assert
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        }

        #endregion

    }
}
