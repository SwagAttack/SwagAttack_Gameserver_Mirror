using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface ILobbyPool
    {
        ILobby GetLobby(string lobbyId);
        bool AddLobby(ILobby lobby);
        bool ReleaseLobby(string lobbyId);
        bool Contains(string lobbyId);
        IEnumerable<ILobby> Find(Func<ILobby, bool> func);
        ILobby FirstOrDefault(Func<ILobby, bool> func);
        ICollection<string> LobbiesCollection { get; }
    }
}