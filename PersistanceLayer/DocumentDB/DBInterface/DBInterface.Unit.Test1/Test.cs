
using Microsoft.Azure.Documents;
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
            _uut.UserRepository.AddUser(_testUser).Wait();
            Assert.That(_testUser.Username == _uut.UserRepository.GetUserByUsername(_testUser.Username).Username);
        }

        [TearDown]
        public void TearDown()
        {
            _uut.UserRepository.DeleteUserByUsername("ab@ab.dk");
        }

    }
}
