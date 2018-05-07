using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Misc;

namespace Application.Interfaces
{
    public class LoggedOutUsersEventArgs : EventArgs
    {
        public LoggedOutUsersEventArgs(BlockingCollection<string> collection)
        {
            LoggedOutUserCollection = collection ?? throw new ArgumentNullException(nameof(collection));
        }
        public BlockingCollection<string> LoggedOutUserCollection { get; }
    }

    public interface IUserCache
    {
        event EventHandler<LoggedOutUsersEventArgs> UsersTimedOutEvent;
        Task AddOrUpdateAsync(string username, string password);
        Task<bool> ConfirmAsync(string username);
        Task<bool> ConfirmAndRefreshAsync(string username, string password);
        Task<bool> RemoveAsync(string username, string password);
    }
}