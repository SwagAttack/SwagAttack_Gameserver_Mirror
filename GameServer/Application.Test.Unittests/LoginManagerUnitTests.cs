using System;
using System.Threading;
using Application.Interfaces;
using Application.Managers;
using Domain.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class LoginManagerUnitTests
    {
        private LoginManager _uut;
        private ITimer _fakeTimer;

        [SetUp]
        public void SetUp()
        {
            _fakeTimer = Substitute.For<ITimer>();
            _uut = new LoginManager(_fakeTimer);
        }

        [Test]
        public void Login_CallingOnce_ContainsUserNameWithTimeStampNow()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            var fakeTime_littleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            var fakeTime_littleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);

            // Act

            _uut.Login(user);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true)); 
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleMore) < 0, Is.EqualTo(true));
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleLess) > 0, Is.EqualTo(true));
        }

        [Test]
        public void Login_CalledTwiceWithSleepInBetween_ContainsUserNameWithUpdatedTimestamp()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act 

            _uut.Login(user);

            Thread.Sleep(15000);

            _uut.Login(user);

            var fakeTime_littleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            var fakeTime_littleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true));
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleMore) < 0, Is.EqualTo(true));
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleLess) > 0, Is.EqualTo(true));
        }

        [Test]
        public void TimerExpired_RestartsTimer()
        {
            // Act
            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);
            // Assert
            _fakeTimer.Received(2).StartWithSeconds(60);
        }

        [Test]
        public void TimerExpired_HasTimedOutUser_RemovesUser()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            var fakeTime = DateTime.Now.AddMinutes(-1);

            _uut.Login(user, fakeTime);

            // Act

            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(false));
        }

        [Test]
        public void TimerExpired_HasUserCloseToExpiring_DoesntRemovesUser()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            var fakeTime = DateTime.Now.AddMinutes(1);
            _uut.Login(user, fakeTime);

            // Act

            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true));
        }

        [Test]
        public void CheckLoginStatus_DoesntContainUser_ReturnsFalse()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act and assert

            Assert.That(_uut.CheckLoginStatus(user.Username), Is.EqualTo(false));
        }

        [Test]
        public void CheckLoginStatus_ContainsUser_ReturnsTrue()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act

            _uut.Login(user);

            // Assert

            Assert.That(_uut.CheckLoginStatus(user.Username), Is.EqualTo(true));
        }

        [Test]
        public void CheckLoginStatus_ContainsUser_UpdatesTimeStampCorrectly()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act

            _uut.Login(user);

            Thread.Sleep(5);

            _uut.Login(user);

            var fakeTime_littleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            var fakeTime_littleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);

            // Assert

            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleMore) < 0, Is.EqualTo(true));
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleLess) > 0, Is.EqualTo(true));
        }

        [TestCase("UsernameOne", "UsernameOne", true)] // Correct user
        [TestCase("UsernameTwo", "UsernameOne", false)] // Invalid user
        public void SubscribeOnLogOut_ValidAndInvalidUserNotPreviouslySubscribedToUserName_ReturnsCorrectly(string usernameToSubscribeTo, string username, bool expected)
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = username;
            _uut.Login(user);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert

            Assert.That(_uut.SubscribeOnLogOut(usernameToSubscribeTo, handle), Is.EqualTo(expected));
        }

        [Test]
        public void SubscribeOnLogOut_PreviouslySubscribedToUserName_ReturnsFalse()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            _uut.Login(user);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act

            _uut.SubscribeOnLogOut(user.Username, handle);

            // Assert

            Assert.That(_uut.SubscribeOnLogOut(user.Username, handle), Is.EqualTo(false));
        }

        [Test]
        public void SubscribeOnLogOut_SameHandlerForTwoUsernames_ReturnsTrue()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            _uut.Login(user);

            var anotherUser = Substitute.For<IUser>();
            anotherUser.Username = "UsernameTwo";
            _uut.Login(anotherUser);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert

            Assert.That(_uut.SubscribeOnLogOut(user.Username, handle), Is.EqualTo(true));
            Assert.That(_uut.SubscribeOnLogOut(anotherUser.Username, handle), Is.EqualTo(true));
        }



        [Test]
        public void UnsubscribeOnLogOut_PreviouslySubscribedToUserName_ReturnsTrue()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            _uut.Login(user);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act 

            _uut.SubscribeOnLogOut(user.Username, handle);

            // Assert

            Assert.That(_uut.UnsubscribeOnLogOut(user.Username, handle), Is.EqualTo(true));
        }

        [Test]
        public void UnsubscribeOnLogOut_NotPreviouslySubscribedToUserName_ReturnsFalse()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            _uut.Login(user);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert Assert

            Assert.That(_uut.UnsubscribeOnLogOut(user.Username, handle), Is.EqualTo(false));
        }

        [Test]
        public void UnsubscribeOnLogOut_InvalidUser_ReturnsFalse()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            _uut.Login(user);

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert Assert

            Assert.That(_uut.UnsubscribeOnLogOut("THIS IS AN INVALID USER", handle), Is.EqualTo(false));
        }

        [Test]
        public void TimerExpired_CallsCorrectHandlersForLoggedOutUsers()
        {
            // Arrange

            var userThatStays = Substitute.For<IUser>();
            userThatStays.Username = "UserThatStays";
            _uut.Login(userThatStays);

            var userThatLeaves = Substitute.For<IUser>();
            userThatLeaves.Username = "LeavingUser";
            _uut.Login(userThatLeaves, DateTime.Now.AddMinutes(-1));

            var userTwoThatLeaves = Substitute.For<IUser>();
            userTwoThatLeaves.Username = "LeavingUserTwo";
            _uut.Login(userTwoThatLeaves, DateTime.Now.AddMinutes(-1));

            int handleOneCount = 0;
            int handleTwoCount = 0;

            void HandleOne(object o, string s)
            {
                handleOneCount++;
            }

            UserLoggedOutHandle handleTwo = (o, s) => { handleTwoCount++; };

            // Act

            _uut.SubscribeOnLogOut(userThatLeaves.Username, HandleOne);
            _uut.SubscribeOnLogOut(userTwoThatLeaves.Username, HandleOne);

            _uut.SubscribeOnLogOut(userThatLeaves.Username, handleTwo);

            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);

            // Assert
            Assert.That(handleTwoCount, Is.EqualTo(1));
            Assert.That(handleOneCount, Is.EqualTo(2));
        }




    }
}