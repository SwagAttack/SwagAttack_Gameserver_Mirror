using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Misc;
using NSubstitute;
using NUnit.Framework;

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

        private Dictionary<string, string> _fakeUsers;

        private bool TimeStampIsUpdatedAccordingToNow(DateTime dateTime)
        {
            var twentyMinutesFromNowLittleMore = DateTime.Now.AddMinutes(20).AddSeconds(1);
            var twentyMinutesFromNowLittleLess = DateTime.Now.AddMinutes(20).AddSeconds(-1);
            return (dateTime.CompareTo(twentyMinutesFromNowLittleMore) < 0) && (dateTime.CompareTo(twentyMinutesFromNowLittleLess) > 0);
        }

        [SetUp]
        public void SetUp()
        {
            _fakeTimer = Substitute.For<ITimer>();
            _uut = new UserCache(_fakeTimer);
            _fakeUsers = FakeUserGenerator.GenerateFakeUsers(50000);
        }

        [Test]
        public void AddOrUpdate_DoesNotContainItem_AddsItem()
        {
            // Act
            _uut.AddOrUpdate(UserOne, UserOnePassword);

            // Assert
            Assert.That(_uut.ExpirationStamps.Count == 1);
            Assert.That(_uut.ExpirationStamps.ContainsKey(UserOne));
        }

        [Test]
        public void AddOrUpdate_DoesNotContainItem_SetsTimeoutTo20MinFromNow()
        {
            // Act
            _uut.AddOrUpdate(UserTwo, UserTwoPassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserTwo];
            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void AddOrUpdate_ContainsItem_SetsTimeoutTo20MinFromNow()
        {
            // Setup
            _uut.AddOrUpdate(UserThree, UserThreePassword);

            Thread.Sleep(5000);

            // Act
            _uut.AddOrUpdate(UserThree, UserThreePassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserThree];

            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void Confirm_ContainsItem_ReturnsTrue()
        {
            // Setup
            _uut.AddOrUpdate(UserThree, UserThreePassword);

            // Act
            var confirmed = _uut.Confirm(UserThree);
            // Assert

            Assert.That(confirmed);
        }

        [Test]
        public void Confirm_DoesNotContainItem_ReturnsFalse()
        {
            // Act
            var confirmed = _uut.Confirm(UserTwo);

            // Assert
            Assert.That(!confirmed);
        }

        [Test]
        public void ConfirmAndRefresh_ContainsItem_UpdatesTimeoutAndReturnsTrue()
        {
            // Setup
            _uut.AddOrUpdate(UserThree, UserThreePassword);

            Thread.Sleep(5000);

            // Act
            var confirmed = _uut.ConfirmAndRefresh(UserThree, UserThreePassword);

            // Assert
            var updatedDateTime = _uut.ExpirationStamps[UserThree];
            Assert.That(confirmed);
            Assert.That(TimeStampIsUpdatedAccordingToNow(updatedDateTime));
        }

        [Test]
        public void ConfirmAndRefresh_DoesNotContainItem_ReturnsFalse()
        {
            // Act
            var confirmed = _uut.ConfirmAndRefresh(UserTwo, UserTwoPassword); // Bad user

            // Assert
            Assert.That(!confirmed);
        }

        [Test]
        public void ConfirmAndRefresh_ContainsUserBadPassWord_ReturnsFalse()
        {
            // Setup
            _uut.AddOrUpdate(UserOne, UserOnePassword);

            // Act
            var confirmed = _uut.ConfirmAndRefresh(UserOne, UserTwoPassword);

            // Assert
            Assert.That(!confirmed);
        }

        [Test]
        public void ConfirmAndRefresh_ContainsUserBadPassWord_DoesNotUpdateTimestamp()
        {
            // Setup
            var now = DateTime.Now;
            _uut.AddOrUpdate(UserOne, UserOnePassword, now);

            // Act
            var confirmed = _uut.ConfirmAndRefresh(UserOne, UserTwoPassword); // Wrong password

            // Assert
            Assert.That(!confirmed);
            Assert.That(_uut.ExpirationStamps[UserOne] == now);
        }


        [Test]
        public void Remove_DoesNotContainItem_ReturnsFalse()
        {
            // Act
            var removed = _uut.Remove(UserOne, UserOnePassword);

            // Assert
            Assert.That(!removed);
        }

        [Test]
        public void Remove_ContainsItem_ReturnsTrueAndRemovesItem()
        {
            // Setup
            _uut.AddOrUpdate(UserThree, UserThreePassword);

            // Act
            var removed = _uut.Remove(UserThree, UserThreePassword);

            // Assert
            Assert.That(removed);
            Assert.That(!_uut.ExpirationStamps.ContainsKey(UserThree));
        }

        [Test]
        public void Timeout_ContainsOutdatedItem_RemovesItem()
        {
            // Setup
            _uut.AddOrUpdate(UserOne, UserOnePassword, DateTime.Now);

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
            _uut.AddOrUpdate(UserOne, UserOnePassword, DateTime.Now.AddSeconds(5));

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
                loggedOutUsers.Add(a.TimedOutUsername);
            };

            _uut.AddOrUpdate(UserOne, UserOnePassword, DateTime.Now.AddSeconds(5));
            _uut.AddOrUpdate(UserTwo, UserTwoPassword, DateTime.Now.AddSeconds(5));
            _uut.AddOrUpdate(UserThree, UserThreePassword, DateTime.Now.AddSeconds(6)); // This item should not be notified

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
                Parallel.ForEach(_fakeUsers, e =>
                {
                    _uut.AddOrUpdate(e.Key, e.Value);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Assert.That(_uut.ExpirationStamps.Count, Is.EqualTo(_fakeUsers.Count));
        }

        [Test]
        public void Timeout_ContainsManyOutDatedUsers_CorrectAmountIsLoggedOut()
        {
            var loggedOutUsers = new List<string>();

            // Subscribing to the event
            _uut.UsersTimedOutEvent += (s, a) =>
            {
                loggedOutUsers.Add(a.TimedOutUsername);
            };

            Parallel.ForEach(_fakeUsers, e =>
            {
                _uut.AddOrUpdate(e.Key, e.Value, DateTime.Now.AddMilliseconds(5));
            });

            // Make sure the users have been added
            Assert.That(_uut.ExpirationStamps.Count, Is.EqualTo(_fakeUsers.Count));

            // Act
            _fakeTimer.ExpiredEvent += Raise.Event();

            // Make sure users have been logged out
            Assert.That(_uut.ExpirationStamps.Count != _fakeUsers.Count);

            // Make sure the correct amount has been logged out
            Assert.That(_uut.ExpirationStamps.Count, Is.EqualTo(_fakeUsers.Count - loggedOutUsers.Count));

            // Make sure the logged out users are not contained in the uut
            foreach (var loggedOutUser in loggedOutUsers)
            {
                Assert.That(!_uut.ExpirationStamps.ContainsKey(loggedOutUser));
            }
        }

    }
}