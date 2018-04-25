using System;
using System.Linq;
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
        /// <param name="user"></param>
        public Lobby(string adminUsername)
        {
            AdminUserName = adminUsername;
            _usernames = new Collection<string>();
            AddUser(adminUsername);
        }

        /// <summary>
        /// Adds a user to the private collection of usernames.
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(string username)
        {
            _usernames.Add(username);
        }

        /// <summary>
        /// Removes a user from the private collection of usernames.
        /// </summary>
        /// <param name="user"></param>
        public void RemoveUser(string username)
        {
            _usernames.Remove(username);
        }

        /// <summary>
        /// Removes the current user assigned AdminUserName and Updates the adminUsername property to the current (next) in the collection. 
        /// </summary>
        public void UpdateAdmin()
        {
            _usernames.Remove(AdminUserName);
            AdminUserName = Usernames.ElementAt(0);
        }
    }
}
