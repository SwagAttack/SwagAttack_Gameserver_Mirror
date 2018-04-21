using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface ILobby
    {
        IReadOnlyCollection<string> Usernames { get; }
        string AdminUserName { get; set; }
        string Id { get; set; }

        void AddUser(IUser user);
        void RemoveUser(IUser user);
        void UpdateAdmin();
    }
}