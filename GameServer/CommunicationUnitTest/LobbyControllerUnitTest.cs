using System.Collections.Generic;
using Domain.Interfaces;
using Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace Communication.Test.Unittests
{
	[TestFixture]
	public class LobbyControllerUnitTest
	{
		private Communication.RESTControllers.LobbyController _uut;
		private Application.Interfaces.ILobbyController _fakelobbyManager;

		[SetUp]
		public void setup()
		{
			_fakelobbyManager = Substitute.For<Application.Interfaces.ILobbyController>();
			_uut = new Communication.RESTControllers.LobbyController(_fakelobbyManager);
		}

		[Test]
		public void GetLobbyAsync_return_correct()
		{
			//Arrange
			string lobbyId = "lobbyid";
			Lobby lobby = new Lobby("test");
			_fakelobbyManager.GetLobbyByIdAsync(lobbyId).Returns(lobby);

			//Ack
			var reply =_uut.GetLobbyAsync(lobbyId);


			//Assert
			Assert.That(lobby.Id, Is.EqualTo(reply.Id));

		}

		[Test]
		public void GetLobbyAsync_return_notFound()
		{
			//Arrange
			string lobbyId = "lobbyid";
			_fakelobbyManager.GetLobbyByIdAsync(Arg.Any<string>()).Returns((ILobby) null);

			//Ack
			var reply = _uut.GetLobbyAsync(lobbyId);

			//Assert
			Assert.AreEqual(Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound, reply.Result);

		}

		[Test]
		public void GetAllLobbiesAsync()
		{
			//Arrange
			ICollection<string> allLobbiesCollection = new List<string>();
			allLobbiesCollection.Add("1");

			_fakelobbyManager.GetAllLobbiesAsync().Returns(allLobbiesCollection);

			//Ack
			var reply = _uut.GetAllLobbiesAsync();

			//Assert
			Assert.AreEqual(allLobbiesCollection,reply);
		}

		[Test]
		public void JoinLobbyAsync_Correct_join()
		{
			//Arragne
			string lobbyid = "lobbyid";
			string username = "Username";
			ILobby lobby = new Lobby("admin");
			_fakelobbyManager.JoinLobbyAsync(lobbyid, username).Returns(lobby);

			//Ack
			var reply = _uut.JoinLobbyAsync(lobbyid, username);

			//Asert
			Assert.AreEqual(lobby.Id,reply.Id);
		}

	}
}
