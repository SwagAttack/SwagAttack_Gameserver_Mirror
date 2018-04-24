using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
	public interface ILobbyController
	{
		IActionResult GetAllLobby([FromHeader] string username, [FromHeader] string password);
		IActionResult PutRemoveUser([FromHeader] string username, [FromHeader] string password, string LobbyId);
		IActionResult PutUserIn([FromHeader] string username, [FromHeader] string password, string LobbyId);
		IActionResult PostCreateLobby([FromHeader] string username, [FromHeader] string password, string LobbyId);
	}
}