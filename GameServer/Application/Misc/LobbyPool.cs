using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Domain.Interfaces;

namespace Application.Misc
{
    public class LobbyPool : ILobbyPool
    {
        private readonly Dictionary<string, ILobby> _lobbyDictionaryPool;

        public LobbyPool()
        {
            _lobbyDictionaryPool = new Dictionary<string, ILobby>();
        }

        public bool AddLobby(ILobby lobby)
        {
            return _lobbyDictionaryPool.TryAdd(lobby.Id, lobby);
        }

        public ILobby GetLobby(string lobbyId)
        {
            _lobbyDictionaryPool.TryGetValue(lobbyId, out var val);
            return val;
        }

        public bool ReleaseLobby(string lobbyId)
        {
            return _lobbyDictionaryPool.Remove(lobbyId);
        }

        public IEnumerable<ILobby> Find(Func<ILobby, bool> expression)
        {
            return _lobbyDictionaryPool.Values.Where(expression);
        }

        public ILobby FirstOrDefault(Func<ILobby, bool> func)
        {
            return _lobbyDictionaryPool.Values.FirstOrDefault(func);
        }

        public bool Contains(string lobbyId)
        {
            return _lobbyDictionaryPool.ContainsKey(lobbyId);
        }

        public ICollection<string> LobbiesCollection => _lobbyDictionaryPool.Keys.ToList();

        public IReadOnlyDictionary<string, ILobby> LobbyDictionary => _lobbyDictionaryPool;
    }
}