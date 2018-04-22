using System;
using System.Collections.Generic;
using System.Linq;
using Application.Misc;
using Domain.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class LobbyPoolUnitTests
    {
        private LobbyPool _uut;

        [SetUp]
        public void SetUp()
        {
            _uut = new LobbyPool();
        }

        [Test]
        public void AddLobby_DoesntContainLobby_ReturnsTrueAndAddsLobby()
        {
            var fakeLobby = Substitute.For<ILobby>();
            fakeLobby.Id = "MyFakeLobby";
            var added = _uut.AddLobby(fakeLobby);
            Assert.That(added);
            Assert.That(_uut.LobbyDictionary.ContainsKey(fakeLobby.Id));
        }

        [Test]
        public void AddLobby_ContainsLobby_ReturnsFalse()
        {
            var fakeLobby = Substitute.For<ILobby>();
            fakeLobby.Id = "MyFakeLobby";
            _uut.AddLobby(fakeLobby);
            var added = _uut.AddLobby(fakeLobby);
            Assert.That(added == false, Is.EqualTo(true));
        }

        [Test]
        public void GetLobby_NoSuchLobby_ReturnsNull()
        {
            var lobby = _uut.GetLobby("LobbyIdGoesHere");
            Assert.That(lobby == null, Is.EqualTo(true));
        }

        [Test]
        public void GetLobby_ContainsLobby_ReturnsLobby()
        {
            var fakeLobby = Substitute.For<ILobby>();
            fakeLobby.Id = "MyFakeLobby";
            _uut.AddLobby(fakeLobby);
            var lobby = _uut.GetLobby(fakeLobby.Id);
            Assert.That(lobby.Id == fakeLobby.Id, Is.EqualTo(true));
        }

        [Test]
        public void ReleaseLobby_NoSuchLobby_ReturnsFalse()
        {
            var removed = _uut.ReleaseLobby("LobbyIdGoesHere");
            Assert.That(!removed);
        }

        [Test]
        public void ReleaseLobby_ContainsLobby_ReturnsTrueAndRemovesLobby()
        {
            var fakeLobby = Substitute.For<ILobby>();
            fakeLobby.Id = "MyFakeLobby";
            _uut.AddLobby(fakeLobby);
            var removed = _uut.ReleaseLobby(fakeLobby.Id);
            Assert.That(removed);
            Assert.That(!_uut.LobbyDictionary.ContainsKey(fakeLobby.Id));
        }

        [TestCase("myfakeLobby1", 1)]
        [TestCase("MyFakeLobby2", 1)]
        [TestCase("MyFakeLobby3", 1)]
        [TestCase("MyFakeLobby4", 0)]
        public void FindLobby_SearchBasedOnLobbyId_FindsTheLobby(string searchId, int numberOfTimesFound)
        {
            var fakeLobby1 = Substitute.For<ILobby>();
            fakeLobby1.Id = "MyFakeLobby1";
            var fakeLobby2 = Substitute.For<ILobby>();
            fakeLobby2.Id = "MyFakeLobby2";
            var fakeLobby3 = Substitute.For<ILobby>();
            fakeLobby3.Id = "MyFakeLobby3";
            _uut.AddLobby(fakeLobby1);
            _uut.AddLobby(fakeLobby2);
            _uut.AddLobby(fakeLobby3);
            var lobbies = _uut.Find(l => string.Equals(l.Id, searchId, StringComparison.CurrentCultureIgnoreCase));
            Assert.That(lobbies.Count() == numberOfTimesFound, Is.EqualTo(true));
        }

        [Test]
        public void FindLobby_SearchBasedOnUsernamesInLobby_ReturnsCorrect()
        {
            var fakeLobby1 = Substitute.For<ILobby>();
            fakeLobby1.Usernames.Returns(new List<string> {"AUsername", "AnotherUser"});
            fakeLobby1.Id = "MyFakeLobby1";
            _uut.AddLobby(fakeLobby1);
            var lobbies = _uut.Find(l => l.Usernames.Contains("AUsername"));
            Assert.That(lobbies.Count() == 1, Is.EqualTo(true));
        }


    }
}