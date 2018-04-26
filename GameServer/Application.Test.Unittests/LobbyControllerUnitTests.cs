using System.Linq;
using System.Threading;
using Application.Controllers;
using Application.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class LobbyControllerUnitTests
    {
        private ILoginManager _fakeLoginManager;
        private LobbyController _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeLoginManager = Substitute.For<ILoginManager>();
            _uut = new LobbyController(_fakeLoginManager);
        }

        #region CreateLobbyAsync

        [Test]
        public void CreateLobbyAsync_LobbyIdValidAndUserNotCurrentlyInAnyLobby_ReturnsCorrectLobby()
        {
            // Arrange

            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(true);

            // Act

            var lobby = _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Result;

            // Assert

            Assert.That(lobby.AdminUserName, Is.EqualTo(USERNAME));
            Assert.That(lobby.Id, Is.EqualTo(LOBBY_ID));
        }

        [Test]
        public void CreateLobbyAsync_LobbyIdValidAndUserNotCurrentlyInAnyLobby_AddsLobbyToInternalDictionary()
        {
            // Arrange

            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(true);

            // Act

            var lobby = _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Result;

            // Assert

            Assert.That(_uut.LobbyDictionary[LOBBY_ID], Is.EqualTo(lobby));
        }

        [Test]
        public void CreateLobbyAsync_LobbyIdExists_ReturnsNull()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var lobby = _uut.CreateLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(lobby, Is.EqualTo(null));
        }

        [Test]
        public void CreateLobbyAsync_UsernameAlreadyInLobby_ReturnsNull()
        {
            // Arrange

            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(false);
            // Act

            var lobby = _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Result;

            // Assert

            Assert.That(lobby, Is.EqualTo(null));
        }

        #endregion

        #region JoinLobbyAsync

        [Test]
        public void JoinLobbyAsync_LobbyExistsAndUserIsNotCurrentlyInALobby_AddsUserToLobbyAndReturnsLobby()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var lobby = _uut.JoinLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(lobby.Usernames.Count, Is.EqualTo(2));
            Assert.That(lobby.Usernames.Contains(ANOTHER_USERNAME) && lobby.Usernames.Contains(USERNAME));
        }

        [Test]
        public void JoinLobbyAsync_LobbyExistsAndUserIsNotCurrentlyInALobby_ContainsUpdatedLobby()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var lobby = _uut.JoinLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(_uut.LobbyDictionary[LOBBY_ID], Is.EqualTo(lobby));
            Assert.That(_uut.LobbyDictionary[LOBBY_ID].Usernames.Contains(USERNAME) && _uut.LobbyDictionary[LOBBY_ID].Usernames.Contains(ANOTHER_USERNAME));
        }

        [Test]
        public void JoinLobbyAssync_LobbyExistsAndUserIsCurrentlyInALobby_ReturnsNull()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";
            string LOBBY_ID = "LobbyId";

            // Only USERNAME can be subscribed to
            _fakeLoginManager.SubscribeOnLogOut(Arg.Is<string>( s => s == USERNAME), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var lobby = _uut.JoinLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(lobby, Is.EqualTo(null));
        }

        [Test]
        public void JoinLobbyAssync_LobbyDoesntExist_ReturnsNull()
        {
            // Arrange

            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            // Only USERNAME can be subscribed to
            _fakeLoginManager.SubscribeOnLogOut(Arg.Is<string>(s => s == USERNAME), Arg.Any<UserLoggedOutHandle>()).Returns(true);

            // Act

            var lobby = _uut.JoinLobbyAsync(LOBBY_ID, USERNAME).Result;

            // Assert

            Assert.That(lobby, Is.EqualTo(null));
        }

        #endregion

        #region LeaveLobbyAsync

        [Test]
        public void LeaveLobbyAsync_LobbyExistsWithMoreThanOneUserAndContainsUser_UserIsRemovedReturnsTrue()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";

            string LOBBY_ID = "LobbyId";

            // Make sure everyone can join the lobby
            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);

            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();
            _uut.JoinLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Wait();

            // Act

            var result = _uut.LeaveLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(result);
            Assert.That(_uut.LobbyDictionary[LOBBY_ID].Usernames.Count, Is.EqualTo(1));
            Assert.That(_uut.LobbyDictionary[LOBBY_ID].Usernames.Contains(ANOTHER_USERNAME), Is.EqualTo(false));
        }

        [Test]
        public void LeaveLobbyAsync_LobbyExistAndContainsOnlyUser_LobbyIsRemovedReturnsTrue()
        {
            // Arrange

            string USERNAME = "Username";

            string LOBBY_ID = "LobbyId";

            // Make sure everyone can join the lobby
            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var result = _uut.LeaveLobbyAsync(LOBBY_ID, USERNAME).Result;

            // Assert

            Assert.That(result);
            Assert.That(!_uut.LobbyDictionary.ContainsKey(LOBBY_ID));
        }

        [Test]
        public void LeaveLobbyAsync_LobbyExistsAndContainsUser_UnsubscribesToUsername()
        {
            // Arrange

            string USERNAME = "Username";

            string LOBBY_ID = "LobbyId";

            // Make sure everyone can join the lobby
            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            _uut.LeaveLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Assert

            _fakeLoginManager.Received(1).UnsubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>());
        }

        [Test]
        public void LeaveLobbyAsync_LobbyExistsAndDoesntContainUser_DoesntUnsubscribeToUsernameReturnsFalse()
        {
            // Arrange

            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";

            string LOBBY_ID = "LobbyId";

            // Make sure everyone can join the lobby
            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act

            var result = _uut.LeaveLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Result;

            // Assert

            Assert.That(!result);
            _fakeLoginManager.DidNotReceive().UnsubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>());
        }

        #endregion

        #region UserLoggedOutEvent

        [Test]
        public void UserLoggedOutEvent_OneLobbyWithOneUserAndUserLogsOut_LobbyIsRemoved()
        {
            // Arrange
            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act 
            _uut.UserLoggedOutEvent(null, USERNAME);
            // Wait for handler to remove user
            Thread.Sleep(500);

            // Assert
            Assert.That(_uut.LobbyDictionary.ContainsKey(LOBBY_ID), Is.EqualTo(false));
        }

        [Test]
        public void UserLoggedOutEvent_OneLobbyWithOneUserAndUserLogsOut_UnsubscribesToUser()
        {
            // Arrange
            string USERNAME = "Username";
            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>()).Returns(true);
            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();

            // Act 
            _uut.UserLoggedOutEvent(null, USERNAME);
            // Wait for handler to remove user
            Thread.Sleep(500);

            // Assert
            _fakeLoginManager.Received(1).UnsubscribeOnLogOut(USERNAME, Arg.Any<UserLoggedOutHandle>());
        }

        [Test]
        public void UserLoggedOutEvent_OneLobbyWithSeveralUsersAndUserLogsOut_UpdatesLobbyCorrectly()
        {
            // Arrange
            string USERNAME = "Username";
            string ANOTHER_USERNAME = "AnotherUsername";
            string THIRD_USERNAME = "ThirdUsername";
            string GOBBENOBBER = "GOBBENOBBER";

            string LOBBY_ID = "LobbyId";

            _fakeLoginManager.SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).Returns(true);

            _uut.CreateLobbyAsync(LOBBY_ID, USERNAME).Wait();
            _uut.JoinLobbyAsync(LOBBY_ID, ANOTHER_USERNAME).Wait();
            _uut.JoinLobbyAsync(LOBBY_ID, THIRD_USERNAME).Wait();
            _uut.JoinLobbyAsync(LOBBY_ID, GOBBENOBBER).Wait();

            // Act 
            _uut.UserLoggedOutEvent(null, GOBBENOBBER);

            // Wait for handler to remove user
            Thread.Sleep(500);

            // Assert
            Assert.That(_uut.LobbyDictionary[LOBBY_ID].Usernames.Count == 3 && !_uut.LobbyDictionary[LOBBY_ID].Usernames.Contains(GOBBENOBBER));
        }

        #endregion

    }
}