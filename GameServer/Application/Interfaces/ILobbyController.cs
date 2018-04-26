using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Interfaces;

namespace Application.Interfaces
{
    public interface ILobbyController
    {
        /// <summary>
        /// Creates a new lobby and returns this
        /// </summary>
        /// <param name="lobbyId">The id of the lobby to create</param>
        /// <param name="adminUsername">The username to be assigned as admin of the lobby</param>
        /// <returns name="ILobby">Returns a new lobby with the passed information if the lobby id isen't taken and if the user is not currently attached to another lobby. Else null.</returns>
        ILobby CreateLobby(string lobbyId, string adminUsername);
        ILobby JoinLobby(string lobbyId, string username);
        bool LeaveLobby(string lobbyId, string username);

        /// <summary>
        /// Might not be useful. Is meant to update a lobby in scenarios where the admin user wishes
        /// to pass on this role to another user.
        /// </summary>
        /// <param name="adminUsername">Username of the current admin of the passed <see cref="ILobby"></see> <see cref="lobby"/></param>
        /// <param name="lobby">The lobby to update</param>
        /// <returns>The updated lobby upon succes. Else null.</returns>
        ILobby UpdateLobby(string adminUsername, ILobby lobby);
        ICollection<string> GetAllLobbies();

        Task<ILobby> CreateLobbyAsync(string lobbyId, string adminUsername);
        Task<ILobby> JoinLobbyAsync(string lobbyId, string username);
        Task<bool> LeaveLobbyAsync(string lobbyid, string username);
        Task<ILobby> UpdateLobbyAsync(string adminUsername, ILobby lobby);
        Task<ICollection<string>> GetAllLobbiesAsync();
    }
}