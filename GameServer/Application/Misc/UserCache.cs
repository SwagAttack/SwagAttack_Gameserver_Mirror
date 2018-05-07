using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using C5;

namespace Application.Misc
{
    /// <summary>
    ///     Implementation for Interval Heap can be found at:
    ///     https://github.com/sestoft/C5/blob/master/C5/heaps/IntervalHeap.cs
    ///     Documentation:
    ///     https://www.itu.dk/research/c5/latest/ITU-TR-2006-76.pdf
    /// </summary>
    public class UserCache : IUserCache
    {
        private const int Timeout = 10; // 10 second timeout
        private readonly ITimer _timer;

        private readonly IntervalHeap<ExpirationMark> _expirationMarks;
        private readonly Dictionary<string, UserLookUp> _userLookUpDictionary;

        private readonly object _lock = new object();

        public UserCache(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _expirationMarks = new IntervalHeap<ExpirationMark>(new ExpirationComparer());
            _userLookUpDictionary = new Dictionary<string, UserLookUp>();

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
                foreach (var exipiration in _expirationMarks) dic.Add(exipiration.Username, exipiration.Expiration);
                return dic;
            }
        }

        #region IUserCache Implementations

        public event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;

        public void AddOrUpdate(string username, string password)
        {
            AddOrUpdate(username, password,
                ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut()));
        }

        public bool Confirm(string username)
        {
            lock(_lock)
            {
                return _userLookUpDictionary.ContainsKey(username);
            }
        }

        public bool ConfirmAndRefresh(string username, string password)
        {
            lock(_lock)
            {
                if (_userLookUpDictionary.TryGetValue(username, out var info) && info.Password == password)
                {
                    _expirationMarks.Replace(info.IndexMark, 
                        ExpirationMark.GetNewMark(username, DateTimeHelper.GetNewTimeOut()));
                    return true;
                }
                return false;
            }
        }

        public bool Remove(string username, string password)
        {
            lock(_lock)
            {
                if (_userLookUpDictionary.TryGetValue(username, out var item))
                {
                    _expirationMarks.Delete(item.IndexMark);
                    _userLookUpDictionary.Remove(username);
                    return true;
                }
                return false;
            }
        }

        public void AddOrUpdate(string username, string password, DateTime timeout)
        {
            AddOrUpdate(username, password, ExpirationMark.GetNewMark(username, timeout));
        }

        #endregion

        #region Utility

        private void AddOrUpdate(string username, string password, ExpirationMark mark)
        {
            lock(_lock)
            {
                IPriorityQueueHandle<ExpirationMark> handle = null;

                if (_userLookUpDictionary.TryGetValue(username, out var item))
                {
                    handle = item.IndexMark;
                    _expirationMarks.Replace(handle, mark);
                }
                else
                {
                    _expirationMarks.Add(ref handle, mark);
                }

                _userLookUpDictionary[username] = new UserLookUp {Password = password, IndexMark = handle};
            }
        }

        #endregion

        #region Timeout Pipeline

        private void TimerTimeout(object sender, EventArgs eventArgs)
        {
            lock(_lock)
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
                while (_expirationMarks.Count != 0 && _expirationMarks.FindMin().Expiration.HasTimeout(now))
                    usersToLogOutCollection.Add(_expirationMarks.DeleteMin().Username);
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
                    _userLookUpDictionary.Remove(user);
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

        private class UserLookUp
        {
            public string Password { get; set; }
            public IPriorityQueueHandle<ExpirationMark> IndexMark { get; set; }
        }

        private class ExpirationMark 
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

        private class ExpirationComparer : IComparer<ExpirationMark>
        {
            public int Compare(ExpirationMark x, ExpirationMark y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

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