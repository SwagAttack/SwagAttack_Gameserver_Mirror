
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Models.Interfaces;
using Models.User;
using NUnit.Framework;
using User = Models.User.User;

namespace DBInterface.Unit.Test1
{
    [TestFixture]
    public class Test
    {
        private UnitOfWork.UnitOfWork _uut;
        private User _testUser;

        [SetUp]
        public void Setup()
        {
            _uut = new UnitOfWork.UnitOfWork(new DbContext());
            _testUser = new User
            {
                Email = "ab@ab.dk",
                GivenName = "TestingName",
                LastName = "TestingLastName",
                Password = "123%%%aaaa",
                Username = "1337User"
            };
        }

        [Test]
        public void AddUserToDb()
        {
            _uut.UserRepository.AddUser(_testUser);
            Assert.That(_testUser.Username == _uut.UserRepository.GetUserByUsername(_testUser.Username).Username);
        }

        [Test]
        public void ReplaceUserInDB()
        {
            _testUser.GivenName = "Replaced";
            _uut.UserRepository.ReplaceUser(_testUser);
            Assert.That(_testUser.GivenName == _uut.UserRepository.GetUserByUsername(_testUser.Username).GivenName);
        }

        [Test]
        public async Task FindUserAsyncDB()
        {
            var user = await _uut.UserRepository.GetUserByUsernameAsync(_testUser.Username);
            Assert.That(user.Id == _testUser.Username);
        }

        [Test]
        public void FindUserInDB()
        {
            var user = _uut.UserRepository.GetUserByUsername(_testUser.Username);
            Assert.That(user.Username == _testUser.Username);
        }

        [Test]
        public void DeleteUserInDB()
        {
            _uut.UserRepository.DeleteUserByUsername(_testUser.Username);
            Assert.That(_uut.UserRepository.GetUserByUsername(_testUser.Username), Is.Null);
            _uut.UserRepository.AddUser(_testUser); // put it up again
        }
        

        [TearDown]
        public void TearDown()
        {
            _uut.UserRepository.DeleteUserByUsername("ab@ab.dk");
        }

    }
}
