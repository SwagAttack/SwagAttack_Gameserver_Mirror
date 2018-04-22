using System.Collections.Generic;
using Application.Interfaces;
using Application.Managers;
using Microsoft.Azure.Documents.Spatial;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class LobbyManagerUnitTests
    {
        private ILoginManager _fakeLoginManager;
        private LobbyManager _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeLoginManager = Substitute.For<ILoginManager>();
            _uut = new LobbyManager(_fakeLoginManager);
        }

        [Test]
        public void CreateLobby_NotSubscribedToUsername_ReturnsLobby()
        {
            // Arrange
            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            var username = "Username";
            var lobbyId = "MyLobbyId";

            // Act
            var lobby = _uut.CreateLobby(lobbyId, username);

            // Assert
            Assert.That(lobby != null, Is.EqualTo(true));
        }

        [Test]
        public void CreateLobby_NotSubscribedToUsername_LobbyHasCorrectAdminUserAndLobbyId()
        {
            // Arrange
            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            var username = "Username";
            var lobbyId = "MyLobbyId";

            // Act
            var lobby = _uut.CreateLobby(lobbyId, username);

            // Assert
            Assert.That(lobby.AdminUserName, Is.EqualTo(username));
            Assert.That(lobby.Id, Is.EqualTo(lobbyId));
        }

        [Test]
        public void CreateLobby_SubscribedToUsername_ReturnsNull()
        {
            // Arrange

            var username = "Username";
            var lobbyId = "MyLobbyId";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Is(username), Arg.Any<UserLoggedOutHandle>()).
                Returns(false);

            // Act
            var lobby = _uut.CreateLobby(lobbyId, username);

            // Assert
            Assert.That(lobby == null, Is.EqualTo(true));
        }

        [Test]
        public void CreateLobby_LobbyIdIsTaken_ReturnsNull()
        {
            // Arrange

            // First lobby
            var username = "Username";
            var lobbyId = "MyLobbyId";

            // Second lobby

            var anotherUsername = "AnotherUsername";
            var sameLobbyId = "MyLobbyId";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            // Act
            var lobby = _uut.CreateLobby(lobbyId, username);
            var anotherLobby = _uut.CreateLobby(sameLobbyId, anotherUsername);

            // Assert
            Assert.That(lobby == null, Is.EqualTo(false));
            Assert.That(anotherLobby == null, Is.EqualTo(true));
        }

        [Test]
        public void GetLobby_LobbyExists_ReturnsLobby()
        {
            // Arrange

            var username = "Username";
            var lobbyId = "MyLobbyId";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _uut.CreateLobby(lobbyId, username);

            // Act
            var lobby = _uut.GetLobby(lobbyId);

            // Assert
            Assert.That(lobby == null, Is.EqualTo(false));
        }

        [Test]
        public void GetLobby_LobbyDoesNotExit_ReturnsNull()
        {
            // Arrange

            var lobbyId = "MyLobbyId";

            // Act
            var lobby = _uut.GetLobby(lobbyId);

            // Assert
            Assert.That(lobby == null, Is.EqualTo(true));
        }

        [Test]
        public void CurrentLobbyCollent_ReturnsAllLobbyIds()
        {
            // Arrange

            // First lobby
            var username1 = "Username1";
            var lobbyId1 = "Lobby1";

            // Second lobby
            var username2 = "Username2";
            var lobbyId2 = "Lobby2";

            // Third lobby
            var username3 = "Username3";
            var lobbyId3 = "Lobby3";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _uut.CreateLobby(lobbyId1, username1);
            _uut.CreateLobby(lobbyId2, username2);
            _uut.CreateLobby(lobbyId3, username3);

            // Act

            var lobbyIds = _uut.CurrentLobbyCollection as List<string>;

            // Assert
            Assert.Contains(lobbyId1, lobbyIds);
            Assert.Contains(lobbyId2, lobbyIds);
            Assert.Contains(lobbyId3, lobbyIds);
        }

        [Test]
        public void AddUserToLobby_NotSubscribedToUsername_ReturnsTrueAndLobbyContainsUser()
        {
            // Arrange

            var username = "Username";
            var lobbyId = "MyLobbyId";

            var usernameToJoin = "JoiningUsername";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _uut.CreateLobby(lobbyId, username);
            // Act
            var added = _uut.AddUserToLobby(lobbyId, usernameToJoin);
            // Assert
            Assert.That(added);
            Assert.That(_uut.ExistingLobbies[lobbyId.ToLower()].Usernames.Count == 2, Is.EqualTo(true));
        }

        [Test]
        public void AddUserToLobby_SubscribedToUsername_ReturnsFalse()
        {
            // Arrange

            var username = "Username";
            var lobbyId = "MyLobbyId";

            var usernameToJoin = "JoiningUsername";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Is<string>( t => t != usernameToJoin ), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _uut.CreateLobby(lobbyId, username);
            // Act
            var added = _uut.AddUserToLobby(lobbyId, usernameToJoin);
            // Assert
            Assert.That(added == false);
        }

        [Test]
        public void AddUserToLobby_BadLobbyId_ReturnsFalse()
        {
            // Arrange

            var usernameToJoin = "Username";
            var lobbyId = "MyLobbyId";

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            // Act (adding user to lobby id that does not exist)
            var added = _uut.AddUserToLobby(lobbyId, usernameToJoin);
            // Assert
            Assert.That(added == false);
        }
    
        [Test]
        public void RemoveUserFromLobby_UserAttached_ReturnsTrueRemovesUser()
        {
            // Arrange

            var usernameToJoin = "Username";
            var lobbyId = "MyLobbyId";

            _fakeLoginManager.
                UnsubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _uut.CreateLobby(lobbyId, usernameToJoin);


            // Act
            var removed = _uut.RemoveUserFromLobby(lobbyId, usernameToJoin);

            // Assert
            Assert.That(removed == true, Is.EqualTo(true));
            Assert.That(_uut.);
        }

    }
}