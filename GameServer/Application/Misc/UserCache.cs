using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Application.Misc
{

    public class UserCache : IUserCache
    {
        private const int Timeout = 10; // 10 second timeout

        private readonly object _lock = new object();

        private readonly CacheList<string> _timeoutList;
        private readonly ITimer _timer;
        private readonly Dictionary<string, UserInfo> _userDictionary;

        public UserCache(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _timeoutList = new CacheList<string>();
            _userDictionary = new Dictionary<string, UserInfo>();

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
                    if (!_userDictionary.ContainsKey(exipiration.Item))
                        throw new InvalidDataException(
                            $"{nameof(_userDictionary)} does not contain item: {exipiration.Item}");
                }

                if (dic.Count != _userDictionary.Count)
                    throw new InvalidDataException($"Return value does not match internal dictionary");
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
                ICacheHandle handle;
                if (_userDictionary.TryGetValue(username, out var item))
                {
                    handle = item.CacheHandle;
                    _timeoutList.Update(handle, timeout);
                }
                else
                {
                    _timeoutList.Add(username, timeout, out handle);
                }

                _userDictionary[username] = new UserInfo {CacheHandle = handle, Password = password};
            }
        }

        public bool Confirm(string username)
        {
            lock (_lock)
            {
                return _userDictionary.ContainsKey(username);
            }
        }

        public bool ConfirmAndRefresh(string username, string password)
        {
            lock (_lock)
            {
                if (_userDictionary.TryGetValue(username, out var item) && item.Password == password)
                {
                    _timeoutList.Update(item.CacheHandle, DateTimeHelper.GetNewTimeOut());
                    return true;
                }

                return false;
            }
        }

        public bool Remove(string username, string password)
        {
            lock (_lock)
            {
                if (_userDictionary.TryGetValue(username, out var item))
                {
                    _timeoutList.Remove(item.CacheHandle);
                    return _userDictionary.Remove(username);
                }

                return false;
            }
        }

        #endregion

        #region Timeout Pipeline

        private void TimerTimeout(object sender, EventArgs eventArgs)
        {
            Task notifier;

            lock (_lock)
            {
                var usersToLogOut = new BlockingCollection<string>();
                var usersToNotify = new BlockingCollection<string>();

                var logoutRunner = GetTimedOutUsersAsync(usersToLogOut);
                var logoutHandler = RemoveTimedOutUsersAsync(usersToLogOut, usersToNotify);
                notifier = NotifySubscribersAsync(usersToNotify);
                Task.WaitAll(logoutRunner, logoutHandler);
            }

            notifier.Wait();
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
                return Task.Run(() =>
                {
                    foreach (var username in usersToNotify.GetConsumingEnumerable()) InvokeUserTimedOutEvent(username);
                });

            return Task.CompletedTask;
        }

        private void InvokeUserTimedOutEvent(string username)
        {
            UsersTimedOutEvent?.Invoke(this, new TimedOutUserEventArgs(username));
        }

        private class UserInfo
        {
            public ICacheHandle CacheHandle { get; set; }
            public string Password { get; set; }
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