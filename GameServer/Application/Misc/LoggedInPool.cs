using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Managers;
using Microsoft.Extensions.Caching.Memory;


namespace Application.Misc
{
    public class LoggedInPool : ILoggedInPool
    {
        private readonly Dictionary<UserKey, DateTime> _expirationStamps;
        private readonly HashSet<string> _loggedInUsers;

        private readonly SmartLock _lock;
        private readonly ITimer _timer;

        public event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;

        /// <summary>
        /// This was added for unit-testing purposes. 
        /// This is not thread-safe and should not be used for anything other than testing
        /// </summary>
        public IReadOnlyCollection<string> LoggedInUsers => _loggedInUsers;
    
        /// <summary>
        /// This was added for unit-testing purposes. 
        /// This is not thread-safe and should not be used for anything other than testing
        /// </summary>
        public IReadOnlyDictionary<string, DateTime> ExpirationStamps
        {
            get
            {
                var dic = new Dictionary<string, DateTime>();
                foreach (var exipiration in _expirationStamps)
                {
                    dic.Add(exipiration.Key.Username, exipiration.Value);
                }

                return dic;
            }
        }

        public LoggedInPool(ITimer timer)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));

            _expirationStamps = new Dictionary<UserKey, DateTime>();
            _loggedInUsers = new HashSet<string>();

            _lock = new SmartLock();

            _timer.ExpiredEvent += TimerTimeout;
            _timer.StartWithSeconds(10);
        }

        private void TimerTimeout(object sender, EventArgs eventArgs)
        {
            var timeOuts = new ConcurrentBag<string>();
            var now = DateTime.Now;

            using (_lock.Lock())
            {
                Parallel.ForEach(_expirationStamps, e =>
                {
                    if (CheckForTimeout(now, e.Value))
                    {
                        timeOuts.Add(e.Key.Username);
                        _loggedInUsers.Remove(e.Key.Username);
                        _expirationStamps.Remove(e.Key);
                    }
                });
            }

            if(timeOuts.Count != 0)
                UsersTimedOutEvent?.Invoke(this, 
                    new LoggedOutUsersEventArgs(timeOuts.ToList()));

            _timer.StartWithSeconds(10);
        }


        public async Task AddOrUpdate(string username, string password, DateTime expiration)
        {
            using (await _lock.LockAsync())
            {
                _expirationStamps[UserKey.CreateKey(username, password)] = expiration;
                _loggedInUsers.Add(username);
            }
        }

        public async Task AddOrUpdateAsync(string username, string password)
        {
            await AddOrUpdate(username, password, DateTimeHelper.GetNewTimeOut());
        }

        public async Task<bool> ConfirmAsync(string username)
        {
            using (await _lock.LockAsync())
            {
                return _loggedInUsers.Contains(username);
            }
        }

        public async Task<bool> ConfirmAndRefreshAsync(string username, string password)
        {
            using (await _lock.LockAsync())
            {
                var key = UserKey.CreateKey(username, password);
                if (_expirationStamps.ContainsKey(key))
                {
                    _expirationStamps[key] = DateTimeHelper.GetNewTimeOut();
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string username, string password)
        {
            using (await _lock.LockAsync())
            {
                if(_expirationStamps.Remove(UserKey.CreateKey(username, password)))
                {
                    _loggedInUsers.Remove(username);
                    return true;
                }

                return false;
            }
        }

        private static bool CheckForTimeout(DateTime now, DateTime markedTime)
        {
            return markedTime.CompareTo(now) < 0;
        }
    }

    internal struct UserKey
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public static UserKey CreateKey(string username, string password)
        {
            return new UserKey{Username = username, Password = password};
        }
    }

    internal static class DateTimeHelper
    {
        public static DateTime GetNewTimeOut()
        {
            return DateTime.Now.AddMinutes(20);
        }
    }
}