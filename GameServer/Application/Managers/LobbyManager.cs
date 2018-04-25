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
        private readonly ILobbyPool _lobbyPool;

        public LobbyManager(ILoginManager loginManager, ILobbyPool lobbyPool)
        {
            _loginManager = loginManager ?? throw new ArgumentNullException(nameof(loginManager));
            _lobbyPool = lobbyPool;

        }
        public ILobby CreateLobby(string lobbyId, string username)
        {

            if (username == null || lobbyId == null)
                return null;

            // If we're already subscribed to user the user is already in a newLobby
            if (!_loginManager.SubscribeOnLogOut(username, UserLoggedOutHandle))
                return null;

            lock (_lobbyLock)
            {
                if (_lobbyPool.Contains(lobbyId))
                    return null;

                var lobby = new Lobby(username) { Id = lobbyId };
                if (_lobbyPool.AddLobby(lobby))
                    return lobby;
            }               
            return null;
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
                var lobby = _lobbyPool.FirstOrDefault(l => l.Usernames.Contains(username));

                if (lobby != null)
                {
                    lobby.RemoveUser(username);
                    if (lobby.Usernames.Count == 0)
                        _lobbyPool.ReleaseLobby(lobby.Id);
                }
            }
            _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutHandle);
        }

        public ILobby GetLobby(string lobbyId)
        {
            if(lobbyId == null) throw new ArgumentNullException(nameof(lobbyId));
            lock (_lobbyLock)
            {
                return _lobbyPool.GetLobby(lobbyId);
            }      
        }

        public ICollection<string> CurrentLobbyCollection
        {
            get
            {
                lock (_lobbyLock)
                {
                    return _lobbyPool.LobbiesCollection;
                }
            }
        }

        public bool AddUserToLobby(string lobbyId, string username)
        {
            if (!_loginManager.SubscribeOnLogOut(username, UserLoggedOutHandle))
                return false;

            lock (_lobbyLock)
            {
                var lobby = _lobbyPool.GetLobby(lobbyId);
                if (lobby == null)
                    return false;

                lobby.AddUser(username);
                return true;
            }
        }

        public bool RemoveUserFromLobby(string lobbyId, string username)
        {
            var succes = false;
           
            lock (_lobbyLock)
            {
                var lobby = _lobbyPool.GetLobby(lobbyId);
                if (lobby != null && lobby.Usernames.Contains(username))
                {
                    succes = true;
                    lobby.RemoveUser(username);
                    if (lobby.Usernames.Count == 0)
                        _lobbyPool.ReleaseLobby(lobbyId);
                }
            }

            if (succes)
                _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutHandle);

            return succes;
        }

        public bool UpdateLobby(string lobbyId, ILobby newLobby)
        {
            if(newLobby == null) throw new ArgumentNullException(nameof(newLobby));
            List<string> outdatedUsers = null;
            List<string> newlyAddedUsers = null;
            lock (_lobbyLock)
            {
                var currentLobby = _lobbyPool.GetLobby(lobbyId);
                if (currentLobby != null && newLobby.Id == currentLobby.Id)
                {
                    outdatedUsers = currentLobby.Usernames.Where(s => !newLobby.Usernames.Contains(s)).ToList();
                    newlyAddedUsers = newLobby.Usernames.Where(s => !currentLobby.Usernames.Contains(s)).ToList();
                    _lobbyPool.ReleaseLobby(lobbyId);
                    _lobbyPool.AddLobby(newLobby);
                }
                else
                {
                    return false;
                }
            }

            foreach (var newlyAddedUser in newlyAddedUsers)
            {
                _loginManager.SubscribeOnLogOut(newlyAddedUser, UserLoggedOutHandle);
            }

            foreach (var outdatedUser in outdatedUsers)
            {
                _loginManager.UnsubscribeOnLogOut(outdatedUser, UserLoggedOutHandle);
            }

            return true;
        }
    }
}