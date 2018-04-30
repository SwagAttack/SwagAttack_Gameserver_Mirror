using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Lobby : ILobby
    {
        /// <summary>
        /// private Collection of strings -> Usernames
        /// </summary>
        private readonly Collection<string> _usernames;

        /// <summary>
        /// public IReadOnlyCollection of strings -> Usernames
        /// </summary>
        public IReadOnlyCollection<string> Usernames => _usernames;

        /// <summary>
        /// The owner of the lobby -> Username
        /// </summary>
        public string AdminUserName { get; set; }

        /// <summary>
        /// The identification or name of the lobby
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// Constructor setups lobby, sets AdminUsername, initiates _usernames and adds a user to the list.
        /// </summary>
        /// <param name="adminUserName"></param>
        public Lobby(string adminUserName)
        {
            AdminUserName = adminUserName ?? throw new ArgumentNullException(nameof(adminUserName));
            _usernames = new Collection<string>();
            AddUser(adminUserName);
        }

        /// <summary>
        /// Adds a user to the private collection of usernames.
        /// </summary>
        /// <param name="username"></param>
        public void AddUser(string username)
        {
            username = username ?? throw new ArgumentNullException(nameof(username));

            _usernames.Add(username);

        }

        /// <summary>
        /// Removes a user from the private collection of usernames.
        /// </summary>
        /// 
        /// <param name="username"></param>
        public void RemoveUser(string username)
        {
            username = username ?? throw new ArgumentNullException(nameof(username));

            _usernames.Remove(username);
        }

        /// <summary>
        /// Removes the current user assigned AdminUserName and Updates the adminUsername property to the current (next) in the collection. 
        /// </summary>
        public void UpdateAdmin(string username)
        {
            username = username ?? throw new ArgumentNullException(nameof(username));

            if (username == AdminUserName)
            {
                _usernames.Remove(AdminUserName);
                AdminUserName = Usernames.GetEnumerator().Current;
            }
        }
    }
}
