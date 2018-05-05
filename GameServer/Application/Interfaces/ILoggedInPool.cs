using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Misc;

namespace Application.Interfaces
{
    public class LoggedOutUsersEventArgs : EventArgs
    {
        private readonly List<string> _loggedOutUsers;
        public LoggedOutUsersEventArgs(List<string> loggedOutUsers)
        {
            _loggedOutUsers = loggedOutUsers;
        }

        public IReadOnlyList<string> LoggedOutUsers => _loggedOutUsers;
    }

    public interface ILoggedInPool
    {
        event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;
        Task AddOrUpdateAsync(string username, string password);
        Task<bool> ConfirmAsync(string username);
        Task<bool> ConfirmAndRefreshAsync(string username, string password);
        Task<bool> RemoveAsync(string username, string password);
    }
}