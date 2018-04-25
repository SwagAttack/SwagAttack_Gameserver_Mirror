using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
	public interface ILobbyController
	{
		IActionResult GetAllLobby([FromHeader] string username);
		IActionResult GetLobbyById([FromHeader] string username, [FromHeader] string LobbyId);
		IActionResult PostCreateLobby([FromHeader] string username, [FromUri] string LobbyId);
		IActionResult PutRemoveUser([FromHeader] string username, [FromUri] string LobbyId);
		IActionResult PutUserIn([FromHeader] string username, [FromUri] string LobbyId);
	}
}