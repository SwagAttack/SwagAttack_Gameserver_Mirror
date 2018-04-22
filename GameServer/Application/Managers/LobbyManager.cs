using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Managers
{
    public class LobbyManager : ILobbyManager
    {
        private readonly object _lobbyLock = new object();

        private readonly ILoginManager _loginManager;
        private readonly Dictionary<string, ILobby> _existingLobbies;

        public IReadOnlyDictionary<string, ILobby> ExistingLobbies
        {
            get
            {
                lock (_lobbyLock)
                {
                    return _existingLobbies;
                }
            }
        }

        public LobbyManager(ILoginManager loginManager)
        {
            _loginManager = loginManager ?? throw new ArgumentNullException(nameof(loginManager));
            _existingLobbies = new Dictionary<string, ILobby>();

        }
        public ILobby CreateLobby(string lobbyId, string username)
        {
            if (username == null || lobbyId == null)
                return null;

            lock (_lobbyLock)
            {
                if (!_existingLobbies.ContainsKey(lobbyId.ToLower()) && _loginManager.SubscribeOnLogOut(username, UserLoggedOutHandle))
                {
                    var lobby = new Lobby(username) {Id = lobbyId};
                    _existingLobbies.Add(lobbyId.ToLower(), lobby);
                    return lobby;
                }

                return null;
            }
        }

        /// <summary>
        /// Is made public for unit testing purposes
        /// </summary>
        /// <param name="o"></param>
        /// <param name="username"></param>
        public void UserLoggedOutHandle(object o, string username)
        {
            lock (_lobbyLock)
            {
                var key = _existingLobbies.FirstOrDefault(kvp => kvp.Value.Usernames.Contains(username)).Key;
                if (key != null)
                {
                    var lobby = _existingLobbies[key];
                    lobby.RemoveUser(username);
                }

                _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutHandle);
            }
        }

        public ILobby GetLobby(string lobbyId)
        {
            if(lobbyId == null) throw new ArgumentNullException(nameof(lobbyId));
            lock (_lobbyLock)
            {
                _existingLobbies.TryGetValue(lobbyId.ToLower(), out var lobby);
                return lobby;
            }
        }

        public ICollection<string> CurrentLobbyCollection
        {
            get
            {
                lock (_lobbyLock)
                {
                    var currentLobbies = new List<string>();
                    foreach (var lobby in _existingLobbies.Values.ToList())
                    {
                        currentLobbies.Add(lobby.Id);
                    }

                    return currentLobbies;
                }
            }
        }

        public bool AddUserToLobby(string lobbyId, string username)
        {
            lock (_lobbyLock)
            {
                if (_existingLobbies.TryGetValue(lobbyId.ToLower(), out var lobby) && _loginManager.SubscribeOnLogOut(username, UserLoggedOutHandle))
                {
                    lobby.AddUser(username);
                    return true;
                }

                return false;
            }
        }

        public bool RemoveUserFromLobby(string lobbyId, string username)
        {
            lock (_lobbyLock)
            {
                if (_existingLobbies.TryGetValue(lobbyId.ToLower(), out var lobby) && _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutHandle))
                {
                    if (lobby.Usernames.Contains(username))
                    {
                        lobby.RemoveUser(username);
                        
                        // If lobby is empty remove lobby?

                        return true;
                    }
                }
                return false;
            }
        }

        public bool UpdateLobby(string lobbyId, string adminUsername, ILobby lobby)
        {
            if(lobby == null) throw new ArgumentNullException(nameof(lobby));
            lock (_lobbyLock)
            {
                if (_existingLobbies.TryGetValue(lobbyId, out var currentLobby))
                {
                    if (string.Equals(currentLobby.AdminUserName, adminUsername, StringComparison.CurrentCultureIgnoreCase))
                    {
                        _existingLobbies[lobbyId] = lobby;
                        return true;
                    }
                }
                return false;
            }
        }
    }
}