using System;
using System.Collections.Generic;
using System.Web.Http.Description;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
    public class LobbyController : Controller, ILobbyController
	{
		[ResponseType(typeof(List<ILobby>))]
		[HttpGet]
		public IActionResult GetAllLobby([FromHeader] string username, [FromHeader] string password)
		{
			List<ILobby> returnList = new List<ILobby>();

			return Ok(returnList);
		}
	    [ResponseType(typeof(List<string>))]
	    [HttpPut("Join", Name = "PutUserIn")]
		public IActionResult PutUserIn([FromHeader] string username, [FromHeader] string password, string LobbyId)
	    {
			//return user in lobby
		    List<string> returnList = new List<string>();

		    return Ok(returnList);
	    }

		[HttpPut("Create", Name = "PostCreateLobby")]
		public IActionResult PostCreateLobby(string username, string password, string LobbyId)
		{
			throw new NotImplementedException();
		}

		[ResponseType(typeof(List<string>))]
	    [HttpPut("Leave", Name = "PutRemoveUser")]
	    public IActionResult PutRemoveUser([FromHeader] string username, [FromHeader] string password, string LobbyId)
	    {
		    //return user in lobby
		    List<string> returnList = new List<string>();

		    return Ok(returnList);
	    }
	}
}