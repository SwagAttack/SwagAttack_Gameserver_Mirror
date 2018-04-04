using Models.User;
using NUnit.Framework;

namespace DBInterface.Unit.Test
{
    [TestFixture]
    public class Class1
    {
        private UnitOfWork.UnitOfWork uut_;
        private User TestUser_;
        [SetUp]
        public void setup()
        {
            uut_ = new UnitOfWork.UnitOfWork(new DbContext());
            TestUser_ = new User
            {
                Email = "ab@ab.dk",
                GivenName = "TestingName",
                LastName = "TestingLastName",
                Password = "123%%%aaaa",
                Username = "1337User"
            };

        }

        [TestCase()]
        public void AddUserToDB()
        {
            uut_._userRepository.AddUser(TestUser_).Wait();
            Assert.That(TestUser_.Email == uut_._userRepository.GetUserByEmail(TestUser_.Email).Email);
        }

        [TearDown]
        public void TearDown()
        {
            uut_._userRepository.DeleteUserByEmail("ab@ab.dk");
        }

    }
}
