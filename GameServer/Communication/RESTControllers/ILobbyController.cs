using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
	public interface ILobbyController
	{
		IActionResult GetAllLobbies();
		IActionResult GetLobby(string lobbyId);
		IActionResult CreateLobby(string lobbyId, string username);
		IActionResult LeaveLobby(string lobbyId, string username);
		IActionResult JoinLobby(string lobbyId, string username);
	}
}