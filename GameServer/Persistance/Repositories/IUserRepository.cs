using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Persistance.Repositories
{
    public interface IUserRepository
    {
        /// <summary>
        /// Adding a user to the userDB, from the user model
        /// </summary>
        /// <param name="thisUser">The user wants added</param>
        /// <returns></returns>
        void AddUser(IUser thisUser);
        /// <summary>
        /// Given the username, a user can be found
        /// </summary>
        /// <param name="username">The string that is the username</param>
        /// <returns>The user that was found</returns>
        IUser GetUserByUsername(string username);
        /// <summary>
        /// Deletion in the database
        /// </summary>
        /// <param name="username">The name of the user to be deleted</param>
        void DeleteUserByUsername(string username);
        /// <summary>
        /// For updates to a user.
        /// </summary>
        /// <param name="thisUser">the updated user</param>
        void ReplaceUser(IUser thisUser);
        /// <summary>
        /// Given the username, a user can be found.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>The user that was found</returns>
        Task<IUser> GetUserByUsernameAsync(string username);
        /// <summary>
        /// Returns IEnumereable of users.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Users that were found to match the predicate expression</returns>
        Task<IEnumerable<IUser>> GetUsersByUsernameAsync(Expression<Func<IUser, bool>> predicate);
        /// <summary>
        /// Updates a user with a given username with the new user information.
        /// </summary>
        /// <param name="username">Username of the user to update/replace</param>
        /// <param name="user">The replacement user.</param>
        /// <returns>The updated user.</returns>
        Task<IUser> ReplaceUserAsync(string username, IUser user);
        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>The user that was added.</returns>
        Task<IUser> AddUserAsync(IUser user);
        /// <summary>
        /// Given a username, delete user in DB with mathing username.
        /// </summary>
        /// <param name="username">The username of the user to delete.</param>
        Task DeleteUserByUsernameAsync(string username);

    }
}
