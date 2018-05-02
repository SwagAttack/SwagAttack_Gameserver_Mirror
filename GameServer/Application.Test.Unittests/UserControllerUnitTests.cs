using Application.Controllers;
using Application.Interfaces;
using Domain.Interfaces;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class UserControllerUnitTests
    {
        private ILoginManager _fakeLoginManager;
        private IUserRepository _fakeRepository;
        private UserController _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeLoginManager = Substitute.For<ILoginManager>();
            _fakeRepository = Substitute.For<IUserRepository>();
            _uut = new UserController(_fakeRepository, _fakeLoginManager);
        }

        [Test]
        public void GetUser_UserExistsPasswordIsCorrect_ReturnsUser()
        {
            // Arrange

            var returnedUser = Substitute.For<IUser>();
            returnedUser.Password = "password";

            _fakeRepository.GetItemAsync(Arg.Any<string>())
                .Returns(returnedUser);

            // Act

            var user = _uut.GetUser("username", "password");

            // Assert

            Assert.That(user == null, Is.EqualTo(false));
        }

        [Test]
        public void GetUser_UserDoesntExist_ReturnsNull()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeRepository.GetItemAsync(Arg.Any<string>())
                .Returns(returnedUser);

            // Act

            var user = _uut.GetUser("username", "password");

            // Assert

            Assert.That(user == null, Is.EqualTo(true));
        }

        [Test]
        public void GetUser_UserExistsPasswordIsWrong_ReturnsNull()
        {
            // Arrange

            var returnedUser = Substitute.For<IUser>();
            returnedUser.Password = "password";

            _fakeRepository.GetItemAsync(Arg.Is("username"))
                .Returns(returnedUser);

            // Act

            var user = _uut.GetUser("username", "wrongPassword");

            // Assert

            Assert.That(user == null, Is.EqualTo(true));
        }

        [Test]
        public void GetUser_UserExistsPasswordIsCorrect_LogsInUser()
        {
            // Arrange

            var returnedUser = Substitute.For<IUser>();
            returnedUser.Password = "password";

            _fakeRepository.GetItemAsync(Arg.Is("username"))
                .Returns(returnedUser);

            // Act

            var user = _uut.GetUser("username", "password");

            // Assert

            _fakeLoginManager.Received().Login(user);
        }

        [Test]
        public void CreateUser_UserAlreadyExists_ReturnsNull()
        {
            // Arrange

            _fakeRepository.CreateItemAsync(Arg.Any<IUser>()).ReturnsNull();

            var user = Substitute.For<IUser>();
            user.Username = "username";

            // Act

            var result = _uut.CreateUser(user);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void CreateUser_UserDoesntExist_ReturnsUser()
        {
            // Arrange

            var user = Substitute.For<IUser>();

            _fakeRepository.CreateItemAsync(Arg.Any<IUser>()).Returns(user);

            // Act

            var result = _uut.CreateUser(user);

            // Assert

            Assert.That(result == user, Is.EqualTo(true));
        }

        [Test]
        public void CreateUser_UserDoesntExist_LogsInUser()
        {
            // Arrange

            var user = Substitute.For<IUser>();

            _fakeRepository.CreateItemAsync(Arg.Any<IUser>()).Returns(user);

            // Act

            var result = _uut.CreateUser(user);

            // Assert

            _fakeLoginManager.Received().Login(result);
        }

        [Test]
        public void UpdateUser_UserDoesNotExist_ReturnsNull()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeRepository.UpdateItemAsync(Arg.Any<string>(), Arg.Any<IUser>()).Returns(returnedUser);

            var replacingUser = Substitute.For<IUser>();
            replacingUser.Username = "username";

            // Act

            var result = _uut.UpdateUser("username", replacingUser);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UsernameDoesNotMatchPassedUser_ReturnsNull()
        {
            // Arrange

            string username = null;

            var user = Substitute.For<IUser>();
            user.Username = "this is not the same username as the one above";

            // Act

            var result = _uut.UpdateUser(username, user);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }
        
        [Test]
        public void UpdateUser_UsernameMatchesPassedUserAndUpdateSucceeds_ReturnsUpdatedUser()
        {
            // Arrange

            var originalUser = Substitute.For<IUser>();
            originalUser.Username = "username";
            originalUser.Email = "anEmail";
           
            var replacingUser = Substitute.For<IUser>();
            replacingUser.Username = "username";
            replacingUser.Email = "ReplacedEmail";

            _fakeRepository.UpdateItemAsync(Arg.Is("username"), Arg.Any<IUser>())
                .Returns(replacingUser);

            // Act

            var result = _uut.UpdateUser("username", originalUser);

            // Assert

            Assert.That(result.Email == replacingUser.Email, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UsernameMatchesPassedUserAndUpdateSucceeds_LogsInUser()
        {
            // Arrange

            IUser returnedUser = Substitute.For<IUser>();
            returnedUser.Username = "username";
            returnedUser.Email = "anEmail";

            _fakeRepository.UpdateItemAsync(Arg.Is("username"), Arg.Any<IUser>())
                .Returns(returnedUser);

            var replacingUser = Substitute.For<IUser>();
            replacingUser.Username = "username";
            replacingUser.Email = "ReplacedEmail";

            // Act

            var result = _uut.UpdateUser("username", replacingUser);

            // Assert

            _fakeLoginManager.Received().Login(result);
        }

    }
}