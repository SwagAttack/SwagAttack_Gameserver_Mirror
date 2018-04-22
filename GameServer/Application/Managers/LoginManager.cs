using System;
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
        public class LoggedInUser
        {
            private string _username;
            private string _password;
            private DateTime _expiring;

            public LoggedInUser(string username, string password, DateTime expiring)
            {
                _username = username;
                _password = password;
                _expiring = expiring;
            }

            public string Username => _username;
            public string Password => _password;
            public DateTime Expiration => _expiring;

            public void Update(string password = null)
            {
                if (password != null)
                    _password = password;

                _expiring = DateTime.Now.AddMinutes(20);
            }

        }

        private static ILoginManager _instance = null;

        public IReadOnlyDictionary<string, LoggedInUser> LoggedInUsers
        {
            get
            {
                lock (_loggedInUsers)
                {
                    return _loggedInUsers;
                }           
            }
        }

        private readonly Dictionary<string, LoggedInUser> _loggedInUsers;
        private readonly Dictionary<string, HashSet<UserLoggedOutHandle>> _listeners;

        private readonly ITimer _timeoutTimer;

        public static ILoginManager GetInstance(ITimer timer = null)
        {

            return _instance ?? (_instance = new LoginManager(timer ?? new CountDownTimer()));
        }

        public void Login(IUser user, DateTime timeout)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(user.Username.ToLower()))
                {
                    var loggedInUser = new LoggedInUser(
                        username: user.Username,
                        password: user.Password,
                        expiring: timeout);

                    _loggedInUsers[user.Username.ToLower()] = loggedInUser;
                }
                else
                {
                    _loggedInUsers[user.Username.ToLower()].Update(user.Password);
                }
            }
        }

        public void Login(IUser user)
        {
            Login(user, DateTime.Now.AddMinutes(20));
        }

        public bool CheckLoginStatus(string username, string password)
        {
            lock(_loggedInUsers)
            {
                if (_loggedInUsers.ContainsKey(username.ToLower()))
                {
                    var loggedInUser = _loggedInUsers[username.ToLower()];
                    if (password == loggedInUser.Password)
                    {
                        loggedInUser.Update();
                        return true;
                    }
                }

                return false;
            }
        }

        public bool SubscribeOnLogOut(string username, UserLoggedOutHandle handle)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(username.ToLower()))
                    return false;
            }

            lock (_listeners)
            {
                if (!_listeners.ContainsKey(username.ToLower()))
                {
                    _listeners.Add(username.ToLower(), new HashSet<UserLoggedOutHandle>());
                }

                return _listeners[username.ToLower()].Add(handle);
            }
        }

        public bool UnsubscribeOnLogOut(string username, UserLoggedOutHandle handle)
        {
            lock (_listeners)
            {
                if (!_listeners.ContainsKey(username.ToLower()))
                    return false;

                if (!_listeners[username.ToLower()].Remove(handle))
                    return false;

                if (_listeners[username.ToLower()].Count == 0)
                    _listeners.Remove(username.ToLower());

                return true;
            }
        }

        public LoginManager(ITimer timeoutTimer)
        {
            _listeners = new Dictionary<string, HashSet<UserLoggedOutHandle>>();
            _loggedInUsers = new Dictionary<string, LoggedInUser>();
            _timeoutTimer = timeoutTimer;

            _timeoutTimer.ExpiredEvent += TimeoutTimerOnExpiredEvent;
            _timeoutTimer.StartWithSeconds(60); // StartWithSeconds for 60 seconds
        }

        private void TimeoutTimerOnExpiredEvent(object sender, EventArgs eventArgs)
        {
            var timer = (ITimer)sender;
            var now = DateTime.Now;

            var loggedOutUsers = new List<string>();

            lock (_loggedInUsers)
            {
                foreach (var user in _loggedInUsers.Values.ToList())
                {
                    // User expired
                    if (user.Expiration.CompareTo(now) < 0)
                    {
                        loggedOutUsers.Add(user.Username);
                        _loggedInUsers.Remove(user.Username.ToLower());                      
                    }
                }
            }

            lock (_listeners)
            {
                foreach (var username in loggedOutUsers)
                {
                    if (_listeners.ContainsKey(username))
                    {
                        foreach (var handler in _listeners[username].ToList())
                        {
                            handler.Invoke(this, username);
                        }
                    }
                }
            }

            timer.StartWithSeconds(60);
        }
    }

}