using System.Threading.Tasks;
using Domain.DbAccess;
using Domain.Interfaces;
using Domain.Models;
using NUnit.Framework;
using Persistance.Setup;

namespace Persistance.Test.Unittests.Local
{
    [TestFixture]
    public class Test
    {
        [SetUp]
        public void Setup()
        {
            _addedUser = false;
            _uut = new UserRepository(new DbContext("SwagTest"), "UserTest"); // DatabaseId Swagtest, DocumentId Usertest

            _testUser = new User
            {
                Email = "ab@ab.dk",
                GivenName = "TestingName",
                LastName = "TestingLastName",
                Password = "123%%%aaaa",
                Username = "1337User"
            };
        }


        [TearDown]
        public void TearDown()
        {
            if (_addedUser) _uut.DeleteItemAsync(_testUser.Username).Wait();
        }

        private UserRepository _uut;
        private IUser _testUser;
        private bool _addedUser;

        [Test]
        public async Task AddUserToDb_UserIsAddedToDb()
        {
            var result = await _uut.CreateItemAsync(_testUser);
            Assert.That(result.Username == _testUser.Username);
            _addedUser = true;
        }


        [Test]
        public async Task DeleteUserInDb_UserDoesNotExistPostExecution()
        {
            var result = await _uut.CreateItemAsync(_testUser); // put it up again
            result = await _uut.DeleteItemAsync(result.Username);

            Assert.That(await _uut.GetItemAsync(_testUser.Username), Is.Null);
        }

        [Test]
        public async Task FindUserInDb_UserExists()
        {
            await _uut.CreateItemAsync(_testUser); // put it up again
            var user = await _uut.GetItemAsync(_testUser.Username);
            Assert.That(user.GivenName, Is.EqualTo(_testUser.GivenName));
            _addedUser = true;
        }

        [Test]
        public async Task ReplaceUserInDb_UserIsReplacedInDb()
        {
            await _uut.CreateItemAsync(_testUser); // put it up again
            _testUser.GivenName = "Replaced";
            await _uut.UpdateItemAsync(_testUser.Username, _testUser);
            Assert.That(_testUser.GivenName == _uut.GetItemAsync(_testUser.Username).Result.GivenName);
            _addedUser = true;
        }
    }
}