using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Models.Interfaces;

namespace DBInterface.Repositories
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
        /// <param name="id">The string that is the username</param>
        /// <returns>The user that was found</returns>
        IUser GetUserByUsername(string id);
        /// <summary>
        /// Deletion in the database
        /// </summary>
        /// <param name="username">The name of the user to be deleted</param>
        void DeleteUserByUsername(string username);
        /// <summary>
        /// for updates to a user.
        /// </summary>
        /// <param name="thisUser">the updated user</param>
        void ReplaceUser(IUser thisUser);

        Task<Document> GetUserByUsernameAsync(string id);

    }
}
