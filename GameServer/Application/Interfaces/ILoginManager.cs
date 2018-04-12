using System;
using Models.Interfaces;

namespace Application.Interfaces
{
    /// <summary>
    /// The handler for 'User Logged-Out' events
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="username"></param>
    public delegate void UserLoggedOutHandle(object obj, string username);

    public interface ILoginManager 
    {
        /// <summary>
        /// Logs in the user
        /// </summary>
        /// <param name="user"></param>
        void Login(IUser user);
        /// <summary>
        /// Returns whether the user with username is logged in. If this is the case it updates the users time stamp
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool CheckLoginStatus(string username);
        /// <summary>
        /// Subscribe to the username with the given handler.
        /// If the user is logged out the handler will get called.
        /// Make sure to call CheckLoginStatus prior to this call
        /// </summary>
        /// <param name="username"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool SubscribeOnLogOut(string username, UserLoggedOutHandle handle);
        /// <summary>
        /// Unsubscribes the handler from the username.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool UnsubscribeOnLogOut(string username, UserLoggedOutHandle handle);
    }
}