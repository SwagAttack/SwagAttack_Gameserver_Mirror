using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface ILobby
    {
        IReadOnlyCollection<string> Usernames { get; }
        string AdminUserName { get; set; }
        
        void AddUser(IUser user);
        void RemoveUser(IUser user);

    }
}