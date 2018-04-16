using Domain.Interfaces;
using NUnit.Framework;
using User = Domain.Models.User;

namespace Persistance.Test.Unittests
{
    [TestFixture]
    public class Test
    {
        private UnitOfWork.UnitOfWork _uut;
        private IUser _testUser;
        private bool _addedUser;

        [SetUp]
        public void Setup()
        {
            _addedUser = false;
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
        public void AddUserToDb_UserIsAddedToDb()
        {
            _uut.UserRepository.AddUser(_testUser);
            Assert.That(_testUser.Username == _uut.UserRepository.GetUserByUsername(_testUser.Username).Username);
            _addedUser = true;
        }

        [Test]
        public void AddUserToDbAsync_UserIsAddedToDb()
        {
            var user = _uut.UserRepository.AddUserAsync(_testUser);
            user.Wait();
            Assert.That(user.Result.Username,Is.EqualTo(_testUser.Username));
            _addedUser = true;
        }

        [Test]
        public void ReplaceUserInDb_UserIsReplacedInDb()
        {
            _uut.UserRepository.AddUser(_testUser);
            _testUser.GivenName = "Replaced";
            _uut.UserRepository.ReplaceUser(_testUser);
            Assert.That(_testUser.GivenName == _uut.UserRepository.GetUserByUsername(_testUser.Username).GivenName);
            _addedUser = true;
        }

        [Test]
        public void ReplaceUserInDbAsync_UserIsReplacedInDb()
        {
            _uut.UserRepository.AddUser(_testUser);
            _testUser.GivenName = "Replaced";
            var replacedUser = _uut.UserRepository.ReplaceUserAsync(_testUser.Username, _testUser);
            replacedUser.Wait();
            Assert.That(replacedUser.Result.GivenName,Is.EqualTo(_testUser.GivenName));
            _addedUser = true;
        }

        [Test]
        public void FindUserInDb_UserExists()
        {
            _uut.UserRepository.AddUser(_testUser);
            var user = _uut.UserRepository.GetUserByUsername(_testUser.Username);
            Assert.That(user.Username == _testUser.Username);
            _addedUser = true;
        }

        [Test]
        public void FindUserInDbAsync_UserExists()
        {
            _uut.UserRepository.AddUser(_testUser);
            var user = _uut.UserRepository.GetUserByUsernameAsync(_testUser.Username);
            user.Wait();
            Assert.That(user.Result.Username == _testUser.Username);
            _addedUser = true;
        }

        [Test]
        public void DeleteUserInDb_UserDoesNotExistPostExecution()
        {
            _uut.UserRepository.AddUser(_testUser); // put it up again
            _uut.UserRepository.DeleteUserByUsername(_testUser.Username);
            Assert.That(_uut.UserRepository.GetUserByUsername(_testUser.Username), Is.Null);

        }

        [Test]
        public void DeleteUserInDbAsync_UserDoesNotExistPostExecution()
        {
            _uut.UserRepository.AddUser(_testUser); // put it up again
            _uut.UserRepository.DeleteUserByUsernameAsync(_testUser.Username).Wait();
            Assert.That(_uut.UserRepository.GetUserByUsername(_testUser.Username), Is.Null);
        }


        [TearDown]
        public void TearDown()
        {
            if (_addedUser)
            {
                _uut.UserRepository.DeleteUserByUsername("1337User");
            }
        }

    }
}
