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

        public UserController(IUserController controller)
        {
            _userController = controller;
        }

        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("Login", Name = "GetUser")]
        [ValidateModelState]
        public IActionResult GetUser([ModelBinder(typeof(FromDtoModelBinder))] LoginDto loginInfo)
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
        
        //[HttpGet(Name = "GetUser")]
        //public IActionResult Get([FromBody] JObject user)
        //{

        //    try
        //    {
        //        var credentials = DtoConverter.GetAuthentication(user);

        //        var username = credentials["username"];
        //        var password = credentials["password"];

        //        var result = _userController.GetUser(username, password);

        //        if(result != null)
        //            return new ObjectResult(result);

        //    }
        //    catch (Exception)
        //    {
                
        //    }

        //    return new NotFoundResult();
        //}

        [HttpPost]
        [ValidateModelState]
        public IActionResult CreateUser([ModelBinder(typeof(FromDtoModelBinder))] User rawUser)
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
        public IActionResult UpdateUser([ModelBinder(typeof(FromDtoModelBinder))] User user)
        {
            var result = _userController.UpdateUser(user.Username, user);

            if (result != null)
            {
                return CreatedAtRoute("GetUser", result);
            }

            return BadRequest();
        }

    }
}