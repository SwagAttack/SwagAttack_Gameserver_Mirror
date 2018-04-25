using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Misc;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Controllers
{
    public class LobbyController : ILobbyController
    {
        private readonly ILoginManager _loginManager;
        private readonly BlockingCollection<string> _loggedOutUsers = new BlockingCollection<string>(new ConcurrentQueue<string>(), 100); // Can hold to 100 names at once
        private readonly SmartLock _smartLock = new SmartLock();

        //private Task _logOutTask;

        private readonly Dictionary<string, ILobby> _lobbyDictionary = new Dictionary<string, ILobby>();
        /// <summary>
        /// Included for unit-testing purposes.
        /// </summary>
        public IReadOnlyDictionary<string, ILobby> LobbyDictionary
        {
            get
            {
                using (var rel = _smartLock.Lock())
                {
                    return _lobbyDictionary;
                }
            }
        }

        public LobbyController(ILoginManager loginManager)
        {
            _loginManager = loginManager ?? throw new ArgumentNullException(nameof(loginManager));
            var logoutThread = new Thread(UserLoggedOutHandler) {IsBackground = true};
            logoutThread.Start();
        }

        #region Sync

        public ILobby CreateLobby(string lobbyId, string adminUsername)
        {
            return CreateLobbyAsync(lobbyId, adminUsername).Result;
        }

        public ILobby JoinLobby(string lobbyId, string username)
        {
            return JoinLobbyAsync(lobbyId, username).Result;
        }

        public void LeaveLobby(string lobbyId, string username)
        {
            LeaveLobbyAsync(lobbyId, username).Wait();
        }

        public ILobby UpdateLobby(string adminUsername, ILobby lobby)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<string> GetAllLobbies()
        {
            return GetAllLobbiesAsync().Result;
        }

        #endregion

        #region Async

        public async Task<ILobby> CreateLobbyAsync(string lobbyId, string adminUsername)
        {
            if (adminUsername == null || lobbyId == null)
                return null;

            using (await _smartLock.LockAsync())
            {
                if (!_lobbyDictionary.ContainsKey(lobbyId) && _loginManager.SubscribeOnLogOut(adminUsername, UserLoggedOutEvent))
                {
                    var lobby = new Lobby(adminUsername) {Id = lobbyId};
                    _lobbyDictionary.Add(lobbyId, lobby);
                    return lobby;
                }

                return null;
            }
        }

        public async Task<ILobby> JoinLobbyAsync(string lobbyId, string username)
        {
            if (username == null || lobbyId == null)
                return null;

            using (await _smartLock.LockAsync())
            {
                if (_lobbyDictionary.ContainsKey(lobbyId) && _loginManager.SubscribeOnLogOut(username, UserLoggedOutEvent))
                {
                    _lobbyDictionary[lobbyId].AddUser(username);
                    return _lobbyDictionary[lobbyId];
                }

                return null;
            }
        }

        public async Task LeaveLobbyAsync(string lobbyId, string username)
        {
            if (username == null || lobbyId == null)
                return;

            var result = false;

            using (await _smartLock.LockAsync())
            {
                if (_lobbyDictionary.TryGetValue(lobbyId, out var lobby))
                {
                    if (lobby.Usernames.Contains(username))
                    {
                        lobby.RemoveUser(username);
                        if (lobby.Usernames.Count == 0) _lobbyDictionary.Remove(lobby.Id);
                        result = true;
                    }
                }
            }

            if(result)
                _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutEvent);
        }

        public Task<ILobby> UpdateLobbyAsync(string adminUsername, ILobby lobby)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<string>> GetAllLobbiesAsync()
        {
            using (await _smartLock.LockAsync())
            {
                return _lobbyDictionary.Keys.ToList();
            }
        }

        #endregion

        private void UserLoggedOutHandler()
        {
            for (;;)
            {
                var username = _loggedOutUsers.Take();
                if (username == null) continue;
                using (_smartLock.Lock())
                {
                    var targetLobby = FindLobbyByUser(username);
                    if (targetLobby != null)
                    {
                        targetLobby.RemoveUser(username);
                        if (targetLobby.Usernames.Count == 0) _lobbyDictionary.Remove(targetLobby.Id);
                    }
                }
                _loginManager.UnsubscribeOnLogOut(username, UserLoggedOutEvent);
            }
        }

        // This was made public for unit testing purposes
        public void UserLoggedOutEvent(object o, string username)
        {
            // Add the username to the list of logged out users
            _loggedOutUsers.Add(username);
        }

        #region Misc

        private ILobby FindLobbyByUser(string username)
        {
            return _lobbyDictionary.Values.FirstOrDefault(l => l.Usernames.Contains(username));
        }

        #endregion


    }
}