using System.Collections.Generic;
using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface ILobbyManager
    {
        ILobby CreateLobby(string lobbyId, string username);
        ILobby GetLobby(string lobbyId);
        ICollection<string> CurrentLobbyCollection { get; }
        bool AddUserToLobby(string lobbyId, string username);
        bool RemoveUserFromLobby(string lobbyId, string username);
        bool UpdateLobby(string lobbyId, ILobby lobby);
    }
}