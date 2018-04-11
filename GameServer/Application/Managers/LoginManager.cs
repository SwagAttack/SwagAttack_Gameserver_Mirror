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

        private readonly ITimer _timeoutTimer;

        public static ILoginManager GetInstance(ITimer timer)
        {
            return _instance ?? (_instance = new LoginManager(timer));
        }

        public bool Login(IUser user, DateTime timeout)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(user.Username))
                {
                    _loggedInUsers[user.Username] = timeout.AddMinutes(20);
                    return true;
                }
                return false;
            }
        }

        public bool Login(IUser user)
        {
            return Login(user, DateTime.Now);
        }

        public bool CheckLoginStatus(IUser user)
        {
            lock (_loggedInUsers)
            {
                if (_loggedInUsers.ContainsKey(user.Username))
                {
                    /* Add five minutes */
                    _loggedInUsers[user.Username] += new TimeSpan(0,5,0);
                    return true;
                }
                   
                return false;
            }
        }

        public LoginManager(ITimer timeoutTimer)
        {
            _loggedInUsers = new Dictionary<string, DateTime>();
            _timeoutTimer = timeoutTimer;

            _timeoutTimer.ExpiredEvent += TimeoutTimerOnExpiredEvent;
            _timeoutTimer.Start(60); // Start for 60 seconds
        }

        private void TimeoutTimerOnExpiredEvent(object sender, EventArgs eventArgs)
        {
            var timer = (ITimer)sender;
            var now = DateTime.Now;

            lock (_loggedInUsers)
            {
                foreach (var loggedInUser in _loggedInUsers.Keys.ToList())
                {
                    // Get the timeout
                    var timeout = _loggedInUsers[loggedInUser];
                    // User expired
                    if (timeout.CompareTo(now) < 0)
                    {
                        _loggedInUsers.Remove(loggedInUser);
                    }
                }
            }

            timer.Start(60);
        }
    }
}