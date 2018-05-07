﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Misc;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;

namespace Application.Test.Unittests
{
    [TestFixture]
    public class UserCacheUnitTests
    {
        private UserCache _uut;
        private ITimer _fakeTimer;

        /// <summary>
        /// Utility names and passwords
        /// </summary>

        private const string UserOne = "UserOne";
        private const string UserOnePassword = "PasswordOne";

        private const string UserTwo = "UserTwo";
        private const string UserTwoPassword = "PasswordTwo";

        private const string UserThree = "UserThree";
        private const string UserThreePassword = "PasswordThree";

        private DateTime _twentyMinutesFromNowLittleMore;
        private DateTime _twentyMinutesFromNowLittleLess;

        private Dictionary<string, string> FakeUsers;

        private bool TimeStampIsUpdatedAccordingToNow(DateTime dateTime)
        {
            _twentyMinutesFromNowLittleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            _twentyMinutesFromNowLittleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);
            return (dateTime.CompareTo(_twentyMinutesFromNowLittleMore) < 0) && (dateTime.CompareTo(_twentyMinutesFromNowLittleLess) > 0);
        }

        [SetUp]
        public void SetUp()
        {
            _fakeTimer = Substitute.For<ITimer>();
            _uut = new UserCache(_fakeTimer);
            FakeUsers = UserCacheTestSetup.FakeUsers;
        }

        [Test]
        public void AddOrUpdateAsync_DoesNotContainItem_AddsItem()
        {
            // Act
            _uut.AddOrUpdateAsync(UserOne, UserOnePassword);

            // Assert
            Assert.That(_uut.ExpirationStamps.Count == 1);
            Assert.That(_uut.ExpirationStamps.ContainsKey(UserOne));
        }

        [Test]
        public void AddOrUpdateAsync_DoesNotContainItem_SetsTimeoutTo20MinFromNow()
        {
            // Act
            _uut.AddOrUpdateAsync(UserTwo, UserTwoPassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserTwo];
            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void AddOrUpdateAsync_ContainsItem_SetsTimeoutTo20MinFromNow()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            Thread.Sleep(5000);

            // Act
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserThree];

            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void ConfirmAsync_ContainsItem_ReturnsTrue()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            // Act
            var confirmed = _uut.ConfirmAsync(UserThree);
            // Assert

            Assert.That(confirmed);
        }

        [Test]
        public void ConfirmAsync_DoesNotContainItem_ReturnsFalse()
        {
            // Act
            var confirmed = _uut.ConfirmAsync(UserTwo);

            // Assert
            Assert.That(!confirmed);
        }

        [Test]
        public void ConfirmAndRefreshAsync_ContainsItem_UpdatesTimeoutAndReturnsTrue()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            Thread.Sleep(5000);

            // Act
            var confirmed = _uut.ConfirmAndRefreshAsync(UserThree, UserThreePassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserThree];
            Assert.That(confirmed);
            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void ConfirmAndRefreshAsync_DoesNotContainItem_ReturnsFalse()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            // Act
            var confirmed = _uut.ConfirmAndRefreshAsync(UserTwo, UserThreePassword); // Bad user

            // Assert
            Assert.That(!confirmed);
        }


        [Test]
        public void RemoveAsync_DoesNotContainItem_ReturnsFalse()
        {
            // Act
            var removed = _uut.RemoveAsync(UserOne, UserOnePassword);

            // Assert
            Assert.That(!removed);
        }

        [Test]
        public void RemoveAsync_ContainsItem_ReturnsTrueAndRemovesItem()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword);

            // Act
            var removed = _uut.RemoveAsync(UserThree, UserThreePassword);

            // Assert
            Assert.That(removed);
            Assert.That(!_uut.ExpirationStamps.ContainsKey(UserThree));
        }

        [Test]
        public void Timeout_ContainsOutdatedItem_RemovesItem()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserOne, UserOnePassword, DateTime.Now);

            Thread.Sleep(5000);

            // Act
            _fakeTimer.ExpiredEvent += Raise.EventWith(null, EventArgs.Empty);

            // Assert
            Assert.That(_uut.ExpirationStamps.Count == 0);
        }

        [Test]
        public void Timeout_ItemCloseToExpiring_ItemIsNotRemoved()
        {
            // Setup
            _uut.AddOrUpdateAsync(UserOne, UserOnePassword, DateTime.Now.AddSeconds(5));

            Thread.Sleep(4000);

            // Act
            _fakeTimer.ExpiredEvent += Raise.EventWith(null, EventArgs.Empty);

            // Assert
            Assert.That(_uut.ExpirationStamps.Count == 1);
        }

        [Test]
        public void Timeout_RestartsTimer()
        {
            _fakeTimer.ClearReceivedCalls();
            // Act
            _fakeTimer.ExpiredEvent += Raise.EventWith(null, EventArgs.Empty);

            // Assert
            _fakeTimer.Received(1).StartWithSeconds(10);
        }

        [Test]
        public void Timeout_ContainsOutdatedItems_NotifiesAboutRemovedItems()
        {
            // Setup

            var loggedOutUsers = new List<string>();

            // Subscribing to the event
            _uut.UsersTimedOutEvent += (s, a) =>
            {
                foreach (var user in a.LoggedOutUserCollection.GetConsumingEnumerable())
                {
                    loggedOutUsers.Add(user);
                }
            };

            _uut.AddOrUpdateAsync(UserOne, UserOnePassword, DateTime.Now.AddSeconds(5));
            _uut.AddOrUpdateAsync(UserTwo, UserTwoPassword, DateTime.Now.AddSeconds(5));
            _uut.AddOrUpdateAsync(UserThree, UserThreePassword, DateTime.Now.AddSeconds(6)); // This item should not be notified

            Thread.Sleep(5200);

            // Act
            _fakeTimer.ExpiredEvent += Raise.Event();

            // Assert
            Assert.That(loggedOutUsers.Count == 2);
            Assert.That(loggedOutUsers.Contains(UserOne) && loggedOutUsers.Contains(UserTwo) && !loggedOutUsers.Contains(UserThree));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        public void AddOrUpdate_AddAlotOfUsersInParallel_TheyAreAllAdded(int i)
        {
            Debug.WriteLine("Sup");
            try
            {
                Parallel.ForEach(FakeUsers, e => { _uut.AddOrUpdateAsync(e.Key, e.Value); });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Assert.That(_uut.ExpirationStamps.Count, Is.EqualTo(FakeUsers.Count));
        }

        [Test]
        public void Timeout_ContainsManyOutDatedUsers_CorrectAmountIsLoggedOut()
        {
            var loggedOutUsers = new List<string>();

            // Subscribing to the event
            _uut.UsersTimedOutEvent += (s, a) =>
            {
                foreach (var user in a.LoggedOutUserCollection.GetConsumingEnumerable())
                {
                    loggedOutUsers.Add(user);
                }
            };

            Parallel.ForEach(FakeUsers, e =>
            {
                _uut.AddOrUpdateAsync(e.Key, e.Value, DateTime.Now.AddSeconds(5));
            });

            _fakeTimer.ExpiredEvent += Raise.Event();

            Assert.That(_uut.ExpirationStamps.Count, Is.EqualTo(FakeUsers.Count - loggedOutUsers.Count));

            foreach (var loggedOutUser in loggedOutUsers)
            {
                Assert.That(!_uut.ExpirationStamps.ContainsKey(loggedOutUser));
            }
        }

    }
}