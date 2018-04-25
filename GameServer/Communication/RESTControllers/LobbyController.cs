using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using Application.Interfaces;
using Communication.Filters;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
    public class LobbyController : Controller, ILobbyController
	{
	    private readonly ILobbyManager _lobbyManager;

	    public LobbyController(ILobbyManager lobbyController)
	    {
		    _lobbyManager = lobbyController;

	    }

	    [ResponseType(typeof(List<ILobby>))]
		[Microsoft.AspNetCore.Mvc.HttpGet]
		[AuthorizationSwag]
		public IActionResult GetAllLobby([FromHeader] string username)
		{
			if (username != null)
			{
				return Ok(_lobbyManager.CurrentLobbyCollection);
			}
			return BadRequest();
		}

	    [Microsoft.AspNetCore.Mvc.HttpGet]
	    [AuthorizationSwag]
	    public IActionResult GetLobbyById([FromHeader] string username,[FromHeader] string LobbyId)
	    {
		    if (username != null)
		    {
			    return Ok(_lobbyManager.GetLobby(LobbyId));
		    }
		    return BadRequest();
	    }

	    [ResponseType(typeof(List<string>))]
	    [Microsoft.AspNetCore.Mvc.HttpPut("Join", Name = "PutUserIn")]
	    [AuthorizationSwag]
		public IActionResult PutUserIn([FromHeader] string username, [FromUri]string LobbyId)
	    {

		    if (username != null)
		    {
			    return Ok(_lobbyManager.AddUserToLobby(LobbyId, username));
		    }
		    return BadRequest();
		}

		[Microsoft.AspNetCore.Mvc.HttpPut("Create", Name = "PostCreateLobby")]
		[AuthorizationSwag]
		public IActionResult PostCreateLobby([FromHeader]string username, [FromUri]string LobbyId)
		{
			if (username != null)
			{
				return Ok(_lobbyManager.CreateLobby(LobbyId, username));
			}
			return BadRequest();
		}

		[ResponseType(typeof(List<string>))]
	    [Microsoft.AspNetCore.Mvc.HttpPut("Leave", Name = "PutRemoveUser")]
		[AuthorizationSwag]
		public IActionResult PutRemoveUser([FromHeader] string username, [FromUri]string LobbyId)
	    {

		    if (username != null)
		    {
				return Ok(_lobbyManager.RemoveUserFromLobby(LobbyId, username));
			}
		    return BadRequest();
			//return user in lobby
			List<string> returnList = new List<string>();

		    return Ok(returnList);
	    }
	}
}