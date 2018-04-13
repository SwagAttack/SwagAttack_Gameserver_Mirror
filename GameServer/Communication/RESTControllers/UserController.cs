using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Domain.Models;
using Communication.Filters;
using Microsoft.AspNetCore.Authorization;

namespace Communication.RESTControllers
{
    // <summary>
    // REST Api User Controller
    // </summary>
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private readonly IUserController _userController;

        public UserController(IUserController controller)
        {
            _userController = controller;
        }

        [HttpGet("{username}/{password}", Name = "GetUser")]
        public IActionResult Get(string username, string password)
        {
            var user = _userController.GetUser(username, password);

            if (user == null)
            {
                return Unauthorized();
            }

            return new ObjectResult(user);
        }

        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            var result = _userController.CreateUser(user);

            if (result != null)
            {
                return CreatedAtRoute("GetUser", new { username = result.Username, password = result.Password }, result);
            }

            return BadRequest("Username already exists");

        }

        [HttpPut("{username}/{password}")]
        public IActionResult Put(string username, string password, [FromBody] User user)
        {
            var result = _userController.UpdateUser(username, password, user);

            if (result != null)
            {
                return CreatedAtRoute("GetUser", new { username = result.Username, password = result.Password }, result);
            }

            return Unauthorized();
        }

    }
}