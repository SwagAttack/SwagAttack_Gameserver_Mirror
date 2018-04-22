using System.Collections.Generic;
using Application.Interfaces;
using Application.Managers;
using Domain.Interfaces;
using Microsoft.Azure.Documents.Spatial;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class LobbyManagerUnitTests
    {
        private ILoginManager _fakeLoginManager;
        private ILobbyPool _fakeLobbyPool;
        private LobbyManager _uut;

        [SetUp]
        public void SetUp()
        {
            _fakeLoginManager = Substitute.For<ILoginManager>();
            _fakeLobbyPool = Substitute.For<ILobbyPool>();
            _uut = new LobbyManager(_fakeLoginManager, _fakeLobbyPool);
        }

        [Test]
        public void CreateLobby_NotSubscribedToUsername_ReturnsLobby()
        {
            // Arrange
            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _fakeLobbyPool.Contains(Arg.Any<string>()).Returns(false);
            _fakeLobbyPool.AddLobby(Arg.Any<ILobby>()).Returns(true);

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

            _fakeLobbyPool.Contains(Arg.Any<string>()).Returns(false);
            _fakeLobbyPool.AddLobby(Arg.Any<ILobby>()).Returns(true);

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

            _fakeLobbyPool.Contains(Arg.Any<string>()).Returns(false);
            _fakeLobbyPool.AddLobby(Arg.Any<ILobby>()).Returns(true);

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

            _fakeLoginManager.
                SubscribeOnLogOut(Arg.Any<string>(), Arg.Any<UserLoggedOutHandle>()).
                Returns(true);

            _fakeLobbyPool.Contains(Arg.Any<string>()).Returns(true);
            _fakeLobbyPool.AddLobby(Arg.Any<ILobby>()).Returns(true);

            // Act
            var lobby = _uut.CreateLobby(lobbyId, username);

            // Assert
            Assert.That(lobby == null, Is.EqualTo(true));
        }

        [Test]
        public void GetLobby_LobbyExists_ReturnsLobby()
        {
            // Arrange

            var username = "Username";
            var lobbyId = "MyLobbyId";

            _fakeLobbyPool.GetLobby(Arg.Any<string>()).Returns(Substitute.For<ILobby>());

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

            _fakeLobbyPool.GetLobby(Arg.Any<string>()).ReturnsNull();

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

            _fakeLobbyPool.LobbiesCollection.Returns(new List<string> {lobbyId1, lobbyId2, lobbyId3});

            // Act

            var lobbyIds = _uut.CurrentLobbyCollection as List<string>;

            // Assert
            Assert.Contains(lobbyId1, lobbyIds);
            Assert.Contains(lobbyId2, lobbyIds);
            Assert.Contains(lobbyId3, lobbyIds);
        }

        [Test]
        public void AddUserToLobby_NotSubscribedToUsername_ReturnsTrue()
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

            _fakeLobbyPool.GetLobby(Arg.Any<string>()).ReturnsNull();

            // Act (adding user to lobby id that does not exist)
            var added = _uut.AddUserToLobby(lobbyId, usernameToJoin);
            // Assert
            Assert.That(added == false);
        }
    

    }
}