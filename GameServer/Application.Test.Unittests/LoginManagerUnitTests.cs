using System;
using System.Threading;
using Application.Interfaces;
using Application.Managers;
using Models.Interfaces;
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

            Thread.Sleep(5);

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

            Assert.That(_uut.CheckLoginStatus(user), Is.EqualTo(false));
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

            Assert.That(_uut.CheckLoginStatus(user), Is.EqualTo(true));
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

            _uut.CheckLoginStatus(user);

            var fakeTime_littleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            var fakeTime_littleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);

            // Assert

            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleMore) < 0, Is.EqualTo(true));
            Assert.That(_uut.LoggedInUsers[user.Username].CompareTo(fakeTime_littleLess) > 0, Is.EqualTo(true));
        }


    }
}