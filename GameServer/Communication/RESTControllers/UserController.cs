using System;
using System.Collections.Generic;
using System.Linq;
using Application.Interfaces;
using Application.Managers;
using Application.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Domain.Models;
using Communication.Filters;
using Communication.JsonConverter;
using Communication.ModelBinders;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

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

        public Dictionary<string, string> AuthToken { get; set; } = new Dictionary<string, string>();

        public UserController(IUserController controller)
        {
            _userController = controller;
        }

        public class LoginDto
        {
            private string _username;
            private string _password;

            public string Username
            {
                get => _username;
                set
                {
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentException("Username must be set");

                    _username = value;
                }
            }

            public string Password
            {
                get => _password;
                set
                {
                    if(string.IsNullOrEmpty(value))
                        throw new ArgumentException("Password must be set");

                    _password = value;
                }
            }
        }

        [HttpPost("Login", Name = "GetUser")]
        [ValidateModelState]
        public IActionResult GetUser([ModelBinder(typeof(FromDto))] LoginDto loginInfo)
        {
            if (loginInfo.Username != null && loginInfo.Password != null)
            {
                var result = _userController.GetUser(loginInfo.Username, loginInfo.Password);

                // User is found and has been logged in
                if (result != null)
                    return new ObjectResult(result);

            }

            return new NotFoundResult();
        }       

        [HttpPost]
        [ValidateModelState]
        public IActionResult CreateUser([ModelBinder(typeof(FromDto))] User rawUser)
        {

            var result = _userController.CreateUser(rawUser);

            if (result != null)
            {
                return CreatedAtRoute("GetUser", result);
            }

            return BadRequest("Username already exists");
        }

        [HttpPut]
        [ValidateModelState(Order = 2)]
        [Authentication(Order = 1)]
        public IActionResult UpdateUser([ModelBinder(typeof(FromDto))] User user)
        {
            if (AuthToken.Count == 2 && AuthToken.ContainsKey("username"))
            {
                var result = _userController.UpdateUser(AuthToken["username"], user);

                if (result != null)
                {
                    return CreatedAtRoute("GetUser", result);
                }
            }
            return BadRequest();
        }

    }
}