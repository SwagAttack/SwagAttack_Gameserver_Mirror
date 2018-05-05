using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Application.Interfaces;
using Application.Misc;
using Domain.Interfaces;

namespace Application.Managers
{
    public class LoginManager : ILoginManager
    {
        private static ILoginManager _instance = null;

        private readonly BlockingCollection<string> _loggedOutUsers =
            new BlockingCollection<string>(new ConcurrentQueue<string>(), 100); // Can hold to 100 names at once

        private readonly Dictionary<string, HashSet<UserLoggedOutHandle>> _listeners;
        private readonly ILoggedInPool _loggedInPool;

        public static ILoginManager GetInstance(ILoggedInPool pool = null)
        {
            return _instance ?? (_instance = new LoginManager(pool ?? new LoggedInPool(new CountDownTimer())));
        }

        public void Login(IUser user)
        {
            _loggedInPool.AddOrUpdateAsync(user.Username, user.Password);
        }

        public bool CheckLoginStatus(string username, string password)
        {
            return _loggedInPool.ConfirmAndRefreshAsync(username, password).Result;
        }

        public bool SubscribeOnLogOut(string username, UserLoggedOutHandle handle)
        {
            if (!_loggedInPool.ConfirmAsync(username).Result)
                return false;

            lock (_listeners)
            {
                if (!_listeners.ContainsKey(username))
                {
                    _listeners.Add(username, new HashSet<UserLoggedOutHandle>());
                }

                return _listeners[username].Add(handle);
            }
        }

        public bool UnsubscribeOnLogOut(string username, UserLoggedOutHandle handle)
        {
            lock (_listeners)
            {
                if (!_listeners.ContainsKey(username))
                    return false;

                if (!_listeners[username].Remove(handle))
                    return false;

                if (_listeners[username].Count == 0)
                    _listeners.Remove(username);

                return true;
            }
        }

        public LoginManager(ILoggedInPool loggedInPool)
        {
            _loggedInPool = loggedInPool ?? throw new ArgumentNullException(nameof(loggedInPool));
            _listeners = new Dictionary<string, HashSet<UserLoggedOutHandle>>();
            _loggedInPool.UsersTimedOutEvent += OnUsersTimedOutHandler;

            var logOutInvokerThread = new Thread(LogOutInvoker) {IsBackground = true };
            logOutInvokerThread.Start();
        }

        private void OnUsersTimedOutHandler(object sender, LoggedOutUsersEventArgs loggedOutUsersEventArgs)
        {
            foreach (var user in loggedOutUsersEventArgs.LoggedOutUsers)
            {
                _loggedOutUsers.Add(user);
            }
        }

        private void LogOutInvoker()
        {
            for (;;)
            {
                var user = _loggedOutUsers.Take();
                if (user == null) continue;
                lock (_listeners)
                {
                    if (_listeners.TryGetValue(user, out var handlers))
                    {
                        foreach (var handler in handlers)
                        {
                            handler.Invoke(null, user);
                        }
                    }
                }
            }
        }
    }

}