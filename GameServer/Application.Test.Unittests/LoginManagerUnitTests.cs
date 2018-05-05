using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        private ILoggedInPool _fakeLoggedInPool;

        [SetUp]
        public void SetUp()
        {
            _fakeLoggedInPool = Substitute.For<ILoggedInPool>();
            _uut = new LoginManager(_fakeLoggedInPool);
        }

        [Test]
        public void Login_AddsUserToLoggedInPool()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            user.Password = "Password";

            // Act

            _uut.Login(user);

            // Assert
            Received.InOrder(() => { _fakeLoggedInPool.AddOrUpdateAsync(user.Username, user.Password); });
        }

        [Test]
        public void CheckLoginStatus_DoesntContainUser_ReturnsFalse()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            user.Password = "Password";

            _fakeLoggedInPool.ConfirmAndRefreshAsync(user.Username, user.Password).Returns(false);

            // Act and assert

            Assert.That(_uut.CheckLoginStatus(user.Username, user.Password), Is.EqualTo(false));
        }

        [Test]
        public void CheckLoginStatus_ContainsUserCorrectPassword_ReturnsTrue()
        {
            // Arrange

            var user = Substitute.For<IUser>();
            user.Username = "Username";
            user.Password = "Password";

            _fakeLoggedInPool.ConfirmAndRefreshAsync(user.Username, user.Password).Returns(true);

            // Act and Assert

            Assert.That(_uut.CheckLoginStatus(user.Username, user.Password), Is.EqualTo(true));
        }

        [Test]
        public void SubscribeOnLogout_UserExistNotPreviouslySubscribed_ReturnsTrue()
        {
            // Arrange

            var username = "Username";
            _fakeLoggedInPool.ConfirmAsync(username).Returns(true);

            var handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert

            Assert.That(_uut.SubscribeOnLogOut(username, handle), Is.EqualTo(true));
        }

        [Test]
        public void SubscribeOnLogout_UserExistsPreviouslySubscribed_ReturnsFalse()
        {
            // Arrange

            var username = "Username";
            _fakeLoggedInPool.ConfirmAsync(username).Returns(true);

            var handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            _uut.SubscribeOnLogOut(username, handle);

            // Act and assert

            Assert.That(_uut.SubscribeOnLogOut(username, handle), Is.EqualTo(false));
        }

        [Test]
        public void SubscribeOnLogout_UserDoesNotExist_ReturnsFalse()
        {
            // Arrange

            var username = "Username";
            _fakeLoggedInPool.ConfirmAsync(username).Returns(false);

            var handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act

            var subscribed = _uut.SubscribeOnLogOut(username, handle);

            // Assert
            Assert.That(!subscribed);
        }

        [Test]
        public void UnsubscribeOnLogOut_PreviouslySubscribedToUserName_ReturnsTrue()
        {
            // Arrange

            var username = "Username";
            _fakeLoggedInPool.ConfirmAsync(username).Returns(true);

            var handle = new UserLoggedOutHandle((o, s) =>{});

            // Act 

            _uut.SubscribeOnLogOut(username, handle);

            // Assert

            Assert.That(_uut.UnsubscribeOnLogOut(username, handle), Is.EqualTo(true));
        }

        [Test]
        public void UnsubscribeOnLogOut_UserDoesNotExistNotPreviouslySubscribedToUserName_ReturnsFalse()
        {
            // Arrange

            var username = "Username";

            UserLoggedOutHandle handle = new UserLoggedOutHandle((o, s) =>
            {

            });

            // Act and assert Assert

            Assert.That(_uut.UnsubscribeOnLogOut(username, handle), Is.EqualTo(false));
        }

        [Test]
        public void UnsubscribeOnLogOut_UserDoesExistNotPreviouslySubscribedToUserName_ReturnsFalse()
        {
            // Arrange

            var username = "Username";

            _fakeLoggedInPool.ConfirmAsync(Arg.Any<string>()).Returns(true);
            _uut.SubscribeOnLogOut(username, (o, s) => { }); // The user exists with a default handle

            var handle = new UserLoggedOutHandle((o, s) => // This handle is not subscribed
            {

            });

            // Act and assert Assert

            Assert.That(_uut.UnsubscribeOnLogOut(username, handle), Is.EqualTo(false));
        }

        [Test]
        public void TimerExpired_CallsCorrectHandlersForLoggedOutUsers()
        {
            // Arrange

            var userThatStays = "UserThatStays";
            var userThatLeaves = "LeavingUser";
            var anotherLeavingUser = "AnotherleavingUser";
            
            _fakeLoggedInPool.ConfirmAsync(Arg.Any<string>()).Returns(true);

            var handleOneCount = 0;
            var handleTwoCount = 0;

            void HandleOne(object o, string username)
            {
                handleOneCount++;
            }

            void HandleTwo(object o, string username)
            {
                handleTwoCount++;
            }

            // Assumming we have one subscriber that subscribes to two usernames - this handle should receive one call
            _uut.SubscribeOnLogOut(userThatStays, HandleOne);
            _uut.SubscribeOnLogOut(userThatLeaves, HandleOne);
            
            // And another handler subscribes to two usernames - this handle should receive two calls
            _uut.SubscribeOnLogOut(userThatLeaves, HandleTwo);
            _uut.SubscribeOnLogOut(anotherLeavingUser, HandleTwo);

            // Act
            _fakeLoggedInPool.UsersTimedOutEvent += Raise.EventWith(null, new LoggedOutUsersEventArgs(new List<string>
            {
                userThatLeaves,
                anotherLeavingUser
            }));
            
            Thread.Sleep(500);

            // Assert
            Assert.That(handleOneCount, Is.EqualTo(1));
            Assert.That(handleTwoCount, Is.EqualTo(2));
        }
    }
}