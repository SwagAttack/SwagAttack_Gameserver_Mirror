
using Models.User;
using NUnit.Framework;

namespace DBInterface.Unit.Test1
{
    [TestFixture]
    public class Test
    {
        private UnitOfWork.UnitOfWork uut_;
        private User _testUser;

        [SetUp]
        public void setup()
        {
            uut_ = new UnitOfWork.UnitOfWork(new DbContext());
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
        public void AddUserToDB()
        {
            uut_.UserRepository.AddUser(_testUser).Wait();
            Assert.That(_testUser.Username == uut_.UserRepository.GetUserByUsername(_testUser.Username).Username);
        }

        [TearDown]
        public void TearDown()
        {
            uut_.UserRepository.DeleteUserByEmail("ab@ab.dk");
        }

    }
}
