﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Application.Interfaces;
using C5;
using Medallion.Collections;

namespace Application.Misc
{
    /// <summary>
    ///     Documentation for the priorioty queue can be found at:
    ///     https://github.com/madelson/MedallionUtilities/blob/master/MedallionPriorityQueue/PriorityQueue.cs
    /// </summary>
    public class UserCache : IUserCache
    {
        private const int Timeout = 10; // 10 second timeout
        private readonly PriorityQueue<ExpirationMark> _marks;
        private readonly ITimer _timer;
        private readonly Dictionary<string, string> _users;

        private IntervalHeap<ExpirationMark> marks;
        private readonly Dictionary<string, UserLookUp> users;

        private readonly object _lckObject = new object();

        public UserCache(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _users = new Dictionary<string, string>();
            _marks = new PriorityQueue<ExpirationMark>(new ExpirationComparer());


            marks = new IntervalHeap<ExpirationMark>(new ExpirationComparer());
            users = new Dictionary<string, UserLookUp>();

            _timer.ExpiredEvent += TimerTimeout;
            _timer.StartWithSeconds(Timeout);


        }

        /// <summary>
        ///     This was added for unit-testing purposes.
        ///     This is not thread-safe and should not be used for anything other than testing
        ///     Will return a dictionary with usernames as keys and the value coresponding to the timeout of the user
        /// </summary>
        public IReadOnlyDictionary<string, DateTime> ExpirationStamps
        {
            get
            {
                var dic = new Dictionary<string, DateTime>();
                foreach (var exipiration in marks) dic.Add(exipiration.Username, exipiration.Expiration);
                return dic;
            }
        }

        #region IUserCache Implementations

        public event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;

        public void AddOrUpdateAsync(string username, string password)
        {
            AddOrUpdateAsync(username, password,
                ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut()));
        }

        public bool ConfirmAsync(string username)
        {
            lock (_lckObject)
            {
                return users.ContainsKey(username);
            }
        }

        public bool ConfirmAndRefreshAsync(string username, string password)
        {
            lock(_lckObject)
            {
                if (users.TryGetValue(username, out var info) && info.Password == password)
                {
                    marks.Replace(info.IndexMark, 
                        ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut()));
                    return true;
                }
                return false;
            }
        }

        public bool RemoveAsync(string username, string password)
        {
            lock(_lckObject)
            {
                if (users.TryGetValue(username, out var item))
                {
                    marks.Delete(item.IndexMark);
                    users.Remove(username);
                    return true;
                }
                return false;
            }
        }

        public void AddOrUpdateAsync(string username, string password, DateTime timeout)
        {
            AddOrUpdateAsync(username, password, ExpirationMark.GetNewMark(username, timeout));
        }

        #endregion

        #region Utility

        private void AddOrUpdateAsync(string username, string password, ExpirationMark mark)
        {
            lock(_lckObject)
            {
                IPriorityQueueHandle<ExpirationMark> handle = null;

                if (users.TryGetValue(username, out var item))
                {
                    handle = item.IndexMark;
                    marks.Replace(handle, mark);
                }
                else
                {
                    marks.Add(ref handle, mark);
                }

                users[username] = new UserLookUp {Password = password, IndexMark = handle};
            }
        }

        #endregion

        #region Timeout Pipeline

        private void TimerTimeout(object sender, EventArgs eventArgs)
        {
            lock(_lckObject)
            {
                var usersToLogOut = new BlockingCollection<string>();
                var usersToNotify = new BlockingCollection<string>();

                var logoutRunner = GetTimedOutUsersAsync(usersToLogOut);
                var logoutHandler = RemoveTimedOutUsersAsync(usersToLogOut, usersToNotify);
                var notifier = NotifySubscribersAsync(usersToNotify);
                Task.WaitAll(logoutRunner, logoutHandler, notifier);
            }

            _timer.StartWithSeconds(Timeout);
        }

        private Task GetTimedOutUsersAsync(BlockingCollection<string> usersToLogOutCollection)
        {
            return Task.Run(() =>
            {
                var now = DateTime.Now;
                while (marks.Count != 0 && marks.FindMin().Expiration.HasTimeout(now))
                    usersToLogOutCollection.Add(marks.DeleteMin().Username);
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

        #endregion

        #region Expiration Utility

        public class UserLookUp
        {
            public string Password { get; set; }
            public IPriorityQueueHandle<ExpirationMark> IndexMark { get; set; }
        }

        public class ExpirationMark 
        {
            private ExpirationMark(string username, DateTime expiration)
            {
                Username = username;
                Expiration = expiration;
            }

            public string Username { get; }
            public DateTime Expiration { get; }

            public static ExpirationMark GetNewMark(string username, DateTime expiration)
            {
                return new ExpirationMark(username, expiration);
            }

        }

        public class ExpirationComparer : IComparer<ExpirationMark>
        {
            public int Compare(ExpirationMark x, ExpirationMark y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                //if (string.IsNullOrEmpty(x.Username) && string.IsNullOrEmpty(y.Username))
                //    return 0;

                //if (string.IsNullOrEmpty(x.Username))
                //    return -1;

                //if (string.IsNullOrEmpty(y.Username))
                //    return 1;

                //if (x.Username == y.Username)
                //    return 0;

                return x.Expiration.CompareTo(y.Expiration);
            }
        }

        #endregion
    }

    internal static class DateTimeHelper
    {
        public static DateTime GetNewTimeOut()
        {
            return DateTime.Now.AddMinutes(20);
        }

        public static bool HasTimeout(this DateTime markedTime, DateTime compareTo)
        {
            return markedTime.CompareTo(compareTo) < 0;
        }
    }
}