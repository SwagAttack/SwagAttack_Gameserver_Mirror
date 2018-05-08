using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Interfaces;

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

        private readonly CacheList<string> _timeoutList;
        private readonly Dictionary<string, string> _userDictionary;

        private readonly object _lock = new object();

        public UserCache(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _timeoutList = new CacheList<string>();
            _userDictionary = new Dictionary<string, string>();

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
                foreach (var exipiration in _timeoutList.Collection)
                {
                    dic.Add(exipiration.Item, exipiration.Expiration);
                    if(!_userDictionary.ContainsKey(exipiration.Item)) throw new InvalidDataException();
                }
                if (dic.Count != _userDictionary.Count) throw new InvalidDataException();
                return dic;
            }
        }

        #region IUserCache Implementations

        public event EventHandler<TimedOutUserEventArgs> UsersTimedOutEvent;

        public void AddOrUpdate(string username, string password)
        {
            AddOrUpdate(username, password, DateTime.Now.AddMinutes(20));
        }

        public void AddOrUpdate(string username, string password, DateTime timeout)
        {
            lock (_lock)
            {
                if (_userDictionary.ContainsKey(username))
                {
                    var index = _timeoutList.Find(username);
                    _timeoutList.Update(index, timeout);
                }
                else
                {
                    _timeoutList.Add(username, timeout);
                }

                _userDictionary[username] = password;
            }
        }

        public bool Confirm(string username)
        {
            lock(_lock)
            {
                return _userDictionary.ContainsKey(username);
            }
        }

        public bool ConfirmAndRefresh(string username, string password)
        {
            lock(_lock)
            {
                if (_userDictionary.TryGetValue(username, out var pass) && pass == password)
                {
                    var index = _timeoutList.Find(username);
                    _timeoutList.Update(index, DateTimeHelper.GetNewTimeOut());
                    return true;
                }
                return false;
            }
        }

        public bool Remove(string username, string password)
        {
            lock(_lock)
            {
                if (_userDictionary.ContainsKey(username))
                {
                    var index = _timeoutList.Find(username);
                    _timeoutList.Remove(index);
                    return _userDictionary.Remove(username);
                }

                return false;
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
                while (_timeoutList.ContainsOutdatedItem(now))
                    usersToLogOutCollection.Add(_timeoutList.RemoveAndGet());
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
                    _userDictionary.Remove(user);
                    usersToNotify.Add(user);
                }

                usersToNotify.CompleteAdding();
            });
        }

        private Task NotifySubscribersAsync(BlockingCollection<string> usersToNotify)
        {
            if (UsersTimedOutEvent != null)
            {
                return Task.Run(() =>
                {
                    foreach (var user in usersToNotify.GetConsumingEnumerable())
                    {
                        UsersTimedOutEvent.Invoke(this, new TimedOutUserEventArgs(user));
                    }
                });
            }

            return Task.CompletedTask;
        }

        #endregion
    }

    internal static class DateTimeHelper
    {
        public static DateTime GetNewTimeOut()
        {
            return DateTime.Now.AddMinutes(20);
        }
    }
}