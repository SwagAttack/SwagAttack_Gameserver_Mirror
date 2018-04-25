using System;
using Application.Interfaces;
using Communication.Filters;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Communication.RESTControllers
{
    // <summary>
    // Http Api User Controller
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

        [HttpGet("Login", Name = "GetUser")]
        [ValidateModelState]
        public IActionResult GetUser([FromHeader] string username, [FromHeader] string password)
        {
            var result = _userController.GetUser(username, password);

            // User is found and has been logged in
            if (result != null)
                return new ObjectResult(result);

            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateModelState]
        public IActionResult CreateUser([FromBody] User rawUser)
        {
            var result = _userController.CreateUser(rawUser);

            if (result != null) return CreatedAtRoute("GetUser", result);

            return BadRequest("Username already exists");
        }

        [HttpPut]
        [ValidateModelState]
        [AuthorizationSwag]
        public IActionResult UpdateUser([FromHeader] string username, [FromBody] User user)
        {
            if (username != null)
            {
                var result = _userController.UpdateUser(username, user);

                if (result != null) return CreatedAtRoute("GetUser", result);
            }

            return BadRequest();
        }
    }
}