using System;
using Communication.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
    // <summary>
    // Http Api Lobby Controller
    // </summary>
    [Produces("application/json")]
    [Route("api/Lobby")]
    [AuthorizationSwag]
    public class LobbyController : Controller, ILobbyController
    {
        private readonly Application.Interfaces.ILobbyController _lobbyController;
        public LobbyController(Application.Interfaces.ILobbyController lobbyController)
        {
            _lobbyController = lobbyController;
        }

        [HttpGet("{lobbyId}", Name = "GetLobby")]
        public IActionResult GetLobby(string lobbyId)
        {
            var lobby = _lobbyController.GetLobbyById(lobbyId);
            if (lobby == null)
                return NotFound();
            return new ObjectResult(lobby);
        }

        [HttpGet]
        public IActionResult GetAllLobbies()
        {
            return new ObjectResult(_lobbyController.GetAllLobbies());
        }

        [HttpPost("Join/{lobbyId}")]
        public IActionResult JoinLobby(string lobbyId, [FromHeader] string username)
        {
            var lobby = _lobbyController.JoinLobby(lobbyId, username);
            if (lobby == null)
                return BadRequest();
            return CreatedAtRoute("GetLobby", new { lobbyId = lobby.Id}, lobby);
        }

        [HttpPost("Leave/{lobbyId}")]
        public IActionResult LeaveLobby(string lobbyId, [FromHeader] string username)
        {
            var result = _lobbyController.LeaveLobby(lobbyId, username);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("Create/{lobbyId}")]
        public IActionResult CreateLobby(string lobbyId, [FromHeader] string username)
        {
            var lobby = _lobbyController.CreateLobby(lobbyId, username);
            if (lobby == null)
                return BadRequest();
            return CreatedAtRoute("GetLobby", new { lobbyId = lobby.Id }, lobby);
        }
    }
}