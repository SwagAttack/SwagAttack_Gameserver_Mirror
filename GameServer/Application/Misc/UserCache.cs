﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using Medallion.Collections;

namespace Application.Misc
{
    public class UserCache : IUserCache
    {
        private readonly SmartLock _lock;
        private readonly PriorityQueue<ExpirationMark> _marks;
        private readonly ITimer _timer;
        private readonly Dictionary<string, string> _users;

        public UserCache(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _users = new Dictionary<string, string>();
            _marks = new PriorityQueue<ExpirationMark>(new ExpirationComparer());

            _lock = new SmartLock();

            _timer.ExpiredEvent += TimerTimeout;
            _timer.StartWithSeconds(10);
        }

        /// <summary>
        ///     This was added for unit-testing purposes.
        ///     This is not thread-safe and should not be used for anything other than testing
        /// </summary>
        public IReadOnlyDictionary<string, DateTime> ExpirationStamps
        {
            get
            {
                var dic = new Dictionary<string, DateTime>();
                foreach (var exipiration in _marks) dic.Add(exipiration.Username, exipiration.Expiration);
                return dic;
            }
        }

        public event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;

        public async Task AddOrUpdateAsync(string username, string password)
        {
            await AddOrUpdateAsync(username, password, DateTimeHelper.GetNewTimeOut());
        }

        public async Task<bool> ConfirmAsync(string username)
        {
            using (await _lock.LockAsync())
            {
                return _users.ContainsKey(username);
            }
        }

        public async Task<bool> ConfirmAndRefreshAsync(string username, string password)
        {
            using (await _lock.LockAsync())
            {
                if (_users.TryGetValue(username, out var pass) && pass == password)
                {
                    var mark = ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut());
                    _marks.Remove(mark);
                    _marks.Add(mark);
                    return true;
                }

                return false;
            }
        }

        public async Task<bool> RemoveAsync(string username, string password)
        {
            using (await _lock.LockAsync())
            {
                if (_users.Remove(username))
                {
                    _marks.Remove(ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut()));
                    return true;
                }

                return false;
            }
        }

        private void TimerTimeout(object sender, EventArgs eventArgs)
        {
            using (_lock.Lock())
            {
                var usersToLogOut = new BlockingCollection<string>();
                var usersToNotify = new BlockingCollection<string>();

                var logoutRunner = GetTimedOutUsersAsync(usersToLogOut);
                var logoutHandler = RemoveTimedOutUsersAsync(usersToLogOut, usersToNotify);
                var notifier = NotifySubscribersAsync(usersToNotify);
                Task.WaitAll(logoutRunner, logoutHandler, notifier);
            }

            _timer.StartWithSeconds(10);
        }

        private Task GetTimedOutUsersAsync(BlockingCollection<string> usersToLogOutCollection)
        {
            return Task.Run(() =>
            {
                while (_marks.Count != 0 && _marks.Peek().Expiration.HasTimeout())
                    usersToLogOutCollection.Add(_marks.Dequeue().Username);
                usersToLogOutCollection.CompleteAdding();
            });
        }

        private Task RemoveTimedOutUsersAsync(BlockingCollection<string> usersToRemove,
            BlockingCollection<string> usersToNotify)
        {
            return Task.Run(() =>
            {
                foreach (var user in usersToRemove.GetConsumingEnumerable())
                {
                    _users.Remove(user);
                    usersToNotify.Add(user);
                }

                usersToNotify.CompleteAdding();
            });
        }

        private Task NotifySubscribersAsync(BlockingCollection<string> usersToNotify)
        {
            return Task.Run(() => { UsersTimedOutEvent?.Invoke(this, new LoggedOutUsersEventArgs(usersToNotify)); });
        }

        public async Task AddOrUpdateAsync(string username, string password, DateTime expiration)
        {
            using (await _lock.LockAsync())
            {
                var mark = ExpirationMark.GetNewMark(username, expiration);
                _marks.Remove(mark);
                _marks.Add(mark);
                _users[username] = password;
            }
        }
    }

    internal class ExpirationMark
    {
        public ExpirationMark(string username, DateTime expiration)
        {
            Username = username;
            Expiration = expiration;
        }

        public string Username { get; }
        public DateTime Expiration { get; set; }

        public static ExpirationMark GetNewMark(string username, DateTime expiration)
        {
            return new ExpirationMark(username, expiration);
        }
    }

    internal class ExpirationComparer : IComparer<ExpirationMark>
    {
        public int Compare(ExpirationMark x, ExpirationMark y)
        {
            return x.Username == y.Username ? 0 : x.Expiration.CompareTo(y.Expiration);
        }
    }

    internal static class DateTimeHelper
    {
        public static DateTime GetNewTimeOut()
        {
            return DateTime.Now.AddMinutes(20);
        }

        public static bool CheckForTimeout(DateTime markedTime)
        {
            return markedTime.CompareTo(DateTime.Now) < 0;
        }

        public static bool HasTimeout(this DateTime markedTime)
        {
            return markedTime.CompareTo(DateTime.Now) < 0;
        }
    }
}