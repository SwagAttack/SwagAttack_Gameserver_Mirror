using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Application.Interfaces;
using Communication.Interfaces;
using Communication.RESTControllers;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Moq;
using ILobbyController = Application.Interfaces.ILobbyController;

namespace Communication.Test.Unittests
{
	[TestFixture]
	public class LobbyControllerUnitTest
	{
		private Communication.RESTControllers.LobbyController _uut;
		private Mock<ILobbyController> _fakelobbyController;

		private Task<IActionResult> _response;

		[SetUp]
		public void setup()
		{
			_fakelobbyController = new Mock<ILobbyController>();
			_uut = new Communication.RESTControllers.LobbyController(_fakelobbyController.Object);
		}

		[Test]
		public async Task GetLobbyAsync_return_correct()
		{
			//Arrange
			string lobbyId = "lobbyid";
			ILobby lobby = new Lobby("test");
			_fakelobbyController.Setup(r =>
					r.GetLobbyByIdAsync(Arg.Any<string>()))
				.Returns(Task.FromResult(lobby));


			//Ack
			var resplyGet = await _uut.GetLobbyAsync(lobbyId);
			var replyObj = resplyGet as Lobby;
		

			//Assert
			Assert.That(lobby.Id, Is.EqualTo(replyObj?.Id));

		}

		[Test]
		public async Task GetLobbyAsync_return_notFound()
		{
			//Arrange
			string lobbyId = "lobbyid";
			_fakelobbyController.Setup(r =>
					r.GetLobbyByIdAsync(Arg.Any<string>()))
				.Returns(Task.FromResult<ILobby>(null));
			
			//Ack
			var resplyGet = await _uut.GetLobbyAsync(lobbyId);
			
			//Assert
			Assert.That(resplyGet, Is.InstanceOf(typeof(NotFoundResult)));

		}

		/*[Test]
		public async Task GetAllLobbiesAsync()
		{
			//Arrange
			ICollection<string> allLobbiesCollection = new List<string>();
			allLobbiesCollection.Add("1");
			allLobbiesCollection.Add("2");

			//_fakelobbyManager.GetAllLobbiesAsync().Returns(allLobbiesCollection);
			_fakelobbyController.Setup(r =>
					r.GetAllLobbiesAsync())
				.ReturnsAsync(allLobbiesCollection);

			//Ack
			var reply = _uut.GetAllLobbiesAsync();
			var replyObj = reply.Result as ObjectResult;

			//Assert
			Assert.That(replyObj.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
		}*/

		//[Test]
		//public void JoinLobbyAsync_Correct_join()
		//{
		//	//Arragne
		//	string lobbyid = "lobbyid";
		//	string username = "Username";
		//	ILobby lobby = new Lobby("admin");
		//	_fakelobbyManager.JoinLobbyAsync(lobbyid, username).Returns(lobby);

		//	//Ack
		//	var reply = _uut.JoinLobbyAsync(lobbyid, username);

		//	//Asert
		//	Assert.AreEqual(lobby.Id,reply.Id);
		//}

	}
}
