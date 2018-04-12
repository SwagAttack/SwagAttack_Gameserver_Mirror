using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Application.Interfaces;
using Models.Interfaces;

namespace Application.Managers
{
    public class LoginManager : ILoginManager
    {
        private static ILoginManager _instance = null;

        public IReadOnlyDictionary<string, DateTime> LoggedInUsers
        {
            get
            {
                lock (_loggedInUsers)
                {
                    return _loggedInUsers;
                }           
            }
        }

        private readonly Dictionary<string, DateTime> _loggedInUsers;
        private readonly Dictionary<string, HashSet<UserLoggedOutHandle>> _listeners;

        private readonly ITimer _timeoutTimer;

        public static ILoginManager GetInstance(ITimer timer)
        {
            return _instance ?? (_instance = new LoginManager(timer));
        }

        public void Login(IUser user, DateTime timeout)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(user.Username))
                {
                    _loggedInUsers[user.Username] = timeout;
                }
                else
                {
                    // Reset timeout
                    _loggedInUsers[user.Username] = DateTime.Now.AddMinutes(20);
                }
            }
        }

        public void Login(IUser user)
        {
            Login(user, DateTime.Now.AddMinutes(20));
        }

        public bool CheckLoginStatus(string username)
        {
            lock (_loggedInUsers)
            {
                if (_loggedInUsers.ContainsKey(username))
                {
                    /* Reset timeout */
                    _loggedInUsers[username] = DateTime.Now.AddMinutes(20);
                    return true;
                }
                   
                return false;
            }
        }

        public bool SubscribeOnLogOut(string username, UserLoggedOutHandle handle)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(username))
                    return false;
            }

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
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(username))
                    return false;
            }

            lock (_listeners)
            {
                if (!_listeners.ContainsKey(username))
                    return false;

                return _listeners[username].Remove(handle);
            }
        }

        public LoginManager(ITimer timeoutTimer)
        {
            _loggedInUsers = new Dictionary<string, DateTime>();
            _listeners = new Dictionary<string, HashSet<UserLoggedOutHandle>>();
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
                foreach (var loggedInUser in _loggedInUsers.Keys.ToList())
                {
                    // Get the timeout
                    var timeout = _loggedInUsers[loggedInUser];
                    // User expired
                    if (timeout.CompareTo(now) < 0)
                    {
                        loggedOutUsers.Add(loggedInUser);
                        _loggedInUsers.Remove(loggedInUser);                      
                    }
                }
            }

            lock (_listeners)
            {
                foreach (var loggedOutUser in loggedOutUsers)
                {
                    if (_listeners.ContainsKey(loggedOutUser))
                    {
                        foreach (var handler in _listeners[loggedOutUser].ToList())
                        {
                            handler.Invoke(this, loggedOutUser);
                        }
                    }
                }
            }

            timer.StartWithSeconds(60);
        }
    }
}