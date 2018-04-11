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
        public void Login_CallingOnce_ContainsUserNameAndReturnsTrue()
        {
            // Setup

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act and assert

            Assert.That(_uut.Login(user), Is.EqualTo(true));  
            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true));  
        }

        [Test]
        public void Login_CalledTwice_ContainsUserNameAndReturnsFalse()
        {
            // Setup

            var user = Substitute.For<IUser>();
            user.Username = "Username";

            // Act 

            _uut.Login(user);

            // Assert

            Assert.That(_uut.Login(user), Is.EqualTo(false));
            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true));
        }

        [Test]
        public void TimerExpired_RestartsTimer()
        {
            // Act
            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);
            // Assert
            _fakeTimer.Received(2).Start(60);
        }

        [Test]
        public void TimerExpired_HasTimedOutUser_RemovesUser()
        {
            // Setup

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            var fakeTime = DateTime.Now.AddMinutes(-21);
            _uut.Login(user, fakeTime);

            // Act

            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(false));
        }

        [Test]
        public void TimerExpired_HasUserCloseToExpiring_DoesntRemovesUser()
        {
            // Setup

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            var fakeTime = DateTime.Now.AddMinutes(-19);
            _uut.Login(user, fakeTime);

            // Act

            _fakeTimer.ExpiredEvent += Raise.EventWith(_fakeTimer, EventArgs.Empty);

            // Assert

            Assert.That(_uut.LoggedInUsers.ContainsKey(user.Username), Is.EqualTo(true));
        }


    }
}