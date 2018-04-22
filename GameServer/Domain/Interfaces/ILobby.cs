using System.Collections.Generic;

namespace Domain.Interfaces
{
    public interface ILobby
    {
        IReadOnlyCollection<string> Usernames { get; }
        string AdminUserName { get; set; }
        string Id { get; set; }      
        void AddUser(string username);
        void RemoveUser(string username);
        void UpdateAdmin(string username);
    }
}