using System.Threading.Tasks;
using Communication.Filters;
using Communication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
    // <summary>
    // Http Api Lobby Controller
    // </summary>
    [Produces("application/json")]
    [Route("api/Lobby")]
    public class LobbyController : Controller
    {
        private readonly Application.Interfaces.ILobbyController _lobbyController;
        public LobbyController(Application.Interfaces.ILobbyController lobbyController)
        {
            _lobbyController = lobbyController;
        }

        // api/Lobby/{lobbyId}
        [HttpGet("{lobbyId}", Name = "GetLobby")]
        public async Task<IActionResult> GetLobbyAsync(string lobbyId)
        {
            var lobby = await _lobbyController.GetLobbyByIdAsync(lobbyId);
            if (lobby == null)
                return NotFound();
            return new ObjectResult(lobby);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLobbiesAsync()
        {
            return new ObjectResult(await _lobbyController.GetAllLobbiesAsync());
        }

        [HttpPost("Join")]
        [AuthorizationSwag]
        public async Task<IActionResult> JoinLobbyAsync([FromQuery]string lobbyId, [FromHeader] string username)
        {
            var lobby = await _lobbyController.JoinLobbyAsync(lobbyId, username);
            if (lobby == null)
                return BadRequest();
            return CreatedAtRoute("GetLobby", new { lobbyId = lobby.Id }, lobby);
        }

        [HttpPost("Leave")]
        [AuthorizationSwag]
        public async Task<IActionResult> LeaveLobbyAsync([FromQuery]string lobbyId, [FromHeader] string username)
        {
            var result = await _lobbyController.LeaveLobbyAsync(lobbyId, username);
            if (!result)
                return BadRequest();
            var parameter = lobbyId;
            return RedirectToRoute("GetLobby", new { lobbyId = parameter });
        }

        [HttpPost("Create")]
        [AuthorizationSwag]
        public async Task<IActionResult> CreateLobbyAsync([FromQuery]string lobbyId, [FromHeader] string username)
        {
            var lobby = await _lobbyController.CreateLobbyAsync(lobbyId, username);
            if (lobby == null)
                return BadRequest();
            return CreatedAtRoute("GetLobby", new { lobbyId = lobby.Id }, lobby);
        }
    }
}