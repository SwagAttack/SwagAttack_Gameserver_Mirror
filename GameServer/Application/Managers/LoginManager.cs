using System;
using System.Collections.Generic;
using System.Threading;
using Application.Interfaces;
using Models.Interfaces;

namespace Application.Managers
{
    public class LoginManager : ILoginManager
    {
        private static LoginManager _instance = null;

        private readonly Dictionary<string, DateTime> _loggedInUsers;

        private readonly Timer _timeoutTimer;
        private int _timeoutCounter;
        private const int MaxTimeoutCount = 60 * 2; /* Every two min */

        
        public ILoginManager GetInstance()
        {
            return _instance ?? (_instance = new LoginManager());
        }

        public bool Login(IUser user)
        {
            lock (_loggedInUsers)
            {
                if (!_loggedInUsers.ContainsKey(user.Username))
                {
                    _loggedInUsers[user.Username] = new DateTime().AddMinutes(20);
                    return true;
                }
                return false;
            }
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

        private LoginManager()
        {
            _loggedInUsers = new Dictionary<string, DateTime>();
            _timeoutTimer = new Timer(HandleTimeout, null, 1000, 1000);
        }

        private void HandleTimeout(object stateInfo)
        {
            if (++_timeoutCounter < MaxTimeoutCount)
            {
                return;
            }
            else
            {
                var now = DateTime.Now;

                lock (_loggedInUsers)
                {
                    foreach (var loggedInUser in _loggedInUsers)
                    {
                        // User expired
                        if(!(loggedInUser.Value.CompareTo(now) < 0))
                        {
                            _loggedInUsers.Remove(loggedInUser.Key);
                        }
                    }
                }
            }
        }
    }
}