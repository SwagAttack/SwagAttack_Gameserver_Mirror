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

        [HttpPost("Login", Name = "GetUser")]
        public IActionResult GetUser([FromBody] JObject rawUser)
        {
            try
            {
                var credentials = DtoConverter.GetAuthentication(rawUser);

                var username = credentials["username"];
                var password = credentials["password"];

                var result = _userController.GetUser(username, password);

                // User is found and has been logged in
                if (result != null)
                    return new ObjectResult(result);

            }
            catch (Exception)
            {

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
        [ValidateModelState(Pattern = typeof(User))]
        public IActionResult CreateUser([FromBody] JObject rawUser)
        {
            try
            {
                IUser user = DtoConverter.ConvertToInstance<User>(rawUser);

                var result = _userController.CreateUser(user);

                if (result != null)
                {
                    return CreatedAtRoute("GetUser", result);
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(ExceptionReader.ReadInnerExceptions(e));
            }


            return BadRequest("Username already exists");

        }

        [HttpPut]
        [ValidateModelState]
        [Authentication]
        public IActionResult UpdateUser([FromBody] JObject rawUser)
        {
            try
            {
                IUser user = DtoConverter.ConvertToInstance<User>(rawUser);

                var result = _userController.UpdateUser(user.Username, user.Password, user);

                if (result != null)
                {
                    return CreatedAtRoute("GetUser", result);
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(ExceptionReader.ReadInnerExceptions(e));
            }

            return BadRequest();
        }

    }
}