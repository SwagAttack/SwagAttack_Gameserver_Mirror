using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.Misc;

namespace Application.Interfaces
{
    public class TimedOutUserEventArgs : EventArgs
    {
        public TimedOutUserEventArgs(string username)
        {
            TimedOutUsername = username;
        }
        public string TimedOutUsername { get; }
    }

    public interface IUserCache
    {
        /// <summary>
        /// Users timeout event. Will contain a blocking collection of usernames that have been logged out.
        /// </summary>
        event EventHandler<TimedOutUserEventArgs> UsersTimedOutEvent;

        /// <summary>
        /// Adds a user to the cache with the default timeout of 20 minutes.
        /// </summary>
        /// <param name="username">Username of user</param>
        /// <param name="password">password of user</param>
        /// <returns></returns>
        void AddOrUpdate(string username, string password);

        /// <summary>
        /// Adds a user to the cache with the timeout given in the paramater <see cref="timeout"/>.
        /// </summary>
        /// <param name="username">Username of user</param>
        /// <param name="password">password of user</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        void AddOrUpdate(string username, string password, DateTime timeout);

        /// <summary>
        /// Confirms that the user is currently in the cache. Does NOT refresh his timeout.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        bool Confirm(string username);

        /// <summary>
        /// Confirms that the user is currenty in the cache and if so refreshes the user's timeout. The password must match.
        /// Returns true if user is found and refreshed, else false.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool ConfirmAndRefresh(string username, string password);

        /// <summary>
        /// Removes the user from the cache.
        /// Returns true if succesful. Else false.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool Remove(string username, string password);
    }
}