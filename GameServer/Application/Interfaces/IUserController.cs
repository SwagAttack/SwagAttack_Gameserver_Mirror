using Models.Interfaces;

namespace Application.Interfaces
{
    public interface IUserController
    {
        /// <summary>
        /// Returns the user with the given username and password. Null if not found 
        /// </summary>
        /// <param name="username">Username of requested user</param>
        /// <param name="password">Password of requested user</param>
        /// <returns>User</returns>
        IUser GetUser(string username, string password);

        /// <summary>
        /// Creates a new user to be stored in the database. Returns valid user upon succes. Otherwise null
        /// </summary>
        /// <param name="user">User to be stored</param>
        /// <returns></returns>
        IUser CreateUser(IUser user);

        /// <summary>
        /// Updates the user as given by username and password. Will return null if unsuccesfull
        /// </summary>
        /// <param name="username">Username of user to be updated</param>
        /// <param name="password">Password of user to be updated</param>
        /// <param name="user">User object containing updated information</param>
        /// <returns></returns>
        IUser UpdateUser(string username, string password, IUser user);
    }
}