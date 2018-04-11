using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Lobby : ILobby
    {
        private readonly Collection<string> _usernames;

        public IReadOnlyCollection<string> Usernames => _usernames;

        public string AdminUserName { get; set; }

        public Lobby(IUser user)
        {
            AdminUserName = user.Username;
            _usernames = new Collection<string>();
            AddUser(user);
        }

        public void AddUser(IUser user)
        {
            _usernames.Add(user.Username);
        }

        public void RemoveUser(IUser user)
        {
            _usernames.Remove(user.Username);
        }
    }
}
