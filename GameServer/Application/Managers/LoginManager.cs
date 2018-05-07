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

        private readonly Dictionary<string, HashSet<UserLoggedOutHandle>> _listeners;
        private readonly BlockingCollection<string> _loggedOutUsers;
        private readonly IUserCache _loggedInPool;

        public static ILoginManager GetInstance(IUserCache pool = null)
        {
            return _instance ?? (_instance = new LoginManager(pool ?? new UserCache(new CountDownTimer())));
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

        public LoginManager(IUserCache loggedInPool)
        {
            _loggedInPool = loggedInPool ?? throw new ArgumentNullException(nameof(loggedInPool));
            _listeners = new Dictionary<string, HashSet<UserLoggedOutHandle>>();
            _loggedOutUsers = new BlockingCollection<string>();

            new Thread(LogOutHandler){IsBackground = true}.Start(); 
            
            _loggedInPool.UsersTimedOutEvent += OnUsersTimedOutHandler;
        }

        private void LogOutHandler()
        {
            for (;;)
            {
                foreach (var loggedOutUser in _loggedOutUsers.GetConsumingEnumerable())
                {
                    if (loggedOutUser == null) continue;
                    lock (_listeners)
                    {
                        if (_listeners.TryGetValue(loggedOutUser, out var handlers))
                        {
                            foreach (var handler in handlers)
                            {
                                handler.Invoke(this, loggedOutUser);
                            }
                        }
                    }
                }
            }
        }

        private void OnUsersTimedOutHandler(object sender, LoggedOutUsersEventArgs loggedOutUsersEventArgs)
        {
            foreach (var user in loggedOutUsersEventArgs.LoggedOutUserCollection.GetConsumingEnumerable())
            {
                _loggedOutUsers.Add(user);
            }
        }
    }

}