using Application.Controllers;
using Application.Interfaces;
using Persistance.UnitOfWork;
using Domain.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class UserControllerUnitTests
    {
        private ILoginManager _fakeLoginManager;
        private IUnitOfWork _fakeUnitOfWork;
        private UserController _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeLoginManager = Substitute.For<ILoginManager>();
            _fakeUnitOfWork = Substitute.For<IUnitOfWork>();
            _uut = new UserController(_fakeUnitOfWork, _fakeLoginManager);
        }

        [Test]
        public void GetUser_UserExistsPasswordIsCorrect_ReturnsUser()
        {
            // Arrange

            var returnedUser = Substitute.For<IUser>();
            returnedUser.Password = "password";

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Any<string>())
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

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Any<string>())
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

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
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

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
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

            var returnedUser = Substitute.For<IUser>();
            returnedUser.Password = "password";

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var anotherUser = Substitute.For<IUser>();
            anotherUser.Username = "username";

            // Act

            var result = _uut.CreateUser(anotherUser);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void CreateUser_UserDoesntExist_ReturnsUser()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var anotherUser = Substitute.For<IUser>();
            anotherUser.Username = "username";

            // Act

            var result = _uut.CreateUser(anotherUser);

            // Assert

            Assert.That(result == anotherUser, Is.EqualTo(true));
        }

        [Test]
        public void CreateUser_UserDoesntExist_AddsUserToRepository()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var anotherUser = Substitute.For<IUser>();
            anotherUser.Username = "username";

            // Act

            var result = _uut.CreateUser(anotherUser);

            // Assert

            _fakeUnitOfWork.UserRepository.Received().AddUser(result);
        }

        [Test]
        public void CreateUser_UserDoesntExist_LogsInUser()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var anotherUser = Substitute.For<IUser>();
            anotherUser.Username = "username";

            // Act

            var result = _uut.CreateUser(anotherUser);

            // Assert

            _fakeLoginManager.Received().Login(result);
        }

        [Test]
        public void UpdateUser_UserDoesntExist_ReturnsNull()
        {
            // Arrange

            IUser returnedUser = null;

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var replacingUser = Substitute.For<IUser>();

            // Act

            var result = _uut.UpdateUser("username", "password", replacingUser);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UserExistsPasswordInvalid_ReturnsNull()
        {
            // Arrange

            IUser returnedUser = Substitute.For<IUser>();
            returnedUser.Username = "username";
            returnedUser.Password = "password";

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            var replacingUser = Substitute.For<IUser>();

            // Act

            var result = _uut.UpdateUser("username", "wrongPassword", replacingUser);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UserExistsPassWordCorrectNotLoggedIn_ReturnsNull()
        {
            // Arrange

            IUser returnedUser = Substitute.For<IUser>();
            returnedUser.Username = "username";
            returnedUser.Password = "password";

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            _fakeLoginManager.CheckLoginStatus(Arg.Any<string>()).Returns(false);

            var replacingUser = Substitute.For<IUser>();

            // Act

            var result = _uut.UpdateUser("username", "password", replacingUser);

            // Assert

            Assert.That(result == null, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UserExistsPassWordCorrectLoggedIn_ReturnsUpdatedUser()
        {
            // Arrange

            IUser returnedUser = Substitute.For<IUser>();
            returnedUser.Username = "username";
            returnedUser.Password = "password";
            returnedUser.Email = "anEmail";
            
            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            _fakeLoginManager.CheckLoginStatus(Arg.Any<string>()).Returns(true);

            var replacingUser = Substitute.For<IUser>();
            replacingUser.Email = "ReplacedEmail";

            // Act

            var result = _uut.UpdateUser("username", "password", replacingUser);

            // Assert

            Assert.That(returnedUser.Email == replacingUser.Email, Is.EqualTo(true));
        }

        [Test]
        public void UpdateUser_UserExistsPassWordCorrectLoggedIn_UpdatesTheUserInTheRepository()
        {
            // Arrange

            IUser returnedUser = Substitute.For<IUser>();
            returnedUser.Username = "username";
            returnedUser.Password = "password";

            _fakeUnitOfWork.UserRepository.GetUserByUsername(Arg.Is("username"))
                .Returns(returnedUser);

            _fakeLoginManager.CheckLoginStatus(Arg.Any<string>()).Returns(true);

            var replacingUser = Substitute.For<IUser>();

            // Act

            var result = _uut.UpdateUser("username", "password", replacingUser);

            // Assert

            _fakeUnitOfWork.UserRepository.Received().ReplaceUser(returnedUser);
        }

    }
}