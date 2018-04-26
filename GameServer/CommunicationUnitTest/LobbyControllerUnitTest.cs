using System;
using System.Collections;
using System.Collections.Generic;
using Application.Interfaces;
using Communication.RESTControllers;
using NSubstitute;
using NUnit.Framework;
using ILobbyController = Application.Interfaces.ILobbyController;

namespace CommunicationUnitTest
{
	[TestFixture]
	public class LobbyControllerUnitTest
	{
		private ILobbyController _uut;
		//private ILobbyManager _fakelobbyManager;
	//	private RestClient _client;

		[SetUp]
		public void setup()
		{
			//_fakelobbyManager = Substitute.For<ILobbyManager>();
			//_uut = new LobbyController(_fakelobbyManager);
			//_client = new RestClient("http://localhost");
		}

/*
		[Test]
		public void GetAllLobby_with_auth_User()
		{
			ICollection<string> replyList = new List<string>();
			replyList.Add("test1");
			replyList.Add("test2");
			_fakelobbyManager.CurrentLobbyCollection.Returns(replyList);

			var result =_uut.GetAllLobby("User");

			Assert.That(result, Is.EquivalentTo(replyList));

		}*/
	}
}
