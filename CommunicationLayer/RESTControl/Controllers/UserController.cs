using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models.User;
using Newtonsoft.Json;
using RESTControl.DAL_Simulation;

namespace RESTControl.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork uow)
        {
            _unitOfWork = uow;

            if (_unitOfWork.Users.Count == 0)
            {

            }
        }

        [HttpGet("{username}/{password}", Name = "GetUser")]
        public IActionResult Get(string username, string password)
        {
            var user = _unitOfWork.Users.SingleOrDefault(u => u.Password == password && u.Username == username);
            if (user == null)
            {
                return NotFound("Wrong username or password");
            }
            return new ObjectResult(user);
        }

        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            if (ModelState.IsValid)
            {
                if (_unitOfWork.Users.FirstOrDefault(u => u.Username == user.Username) == null)
                {
                    _unitOfWork.Users.Add(user);
                }

                return CreatedAtRoute("GetUser", new { username = user.Username, password = user.Password }, user);
            }
            else
            {
                return BadRequest("Not a valid user");
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                var us = _unitOfWork.Users.FirstOrDefault(u => u.Username == user.Username);
                if (us != null)
                {
                    
                }

                return CreatedAtRoute("GetUser", new { username = user.Username, password = user.Password }, user);
            }
            else
            {
                return BadRequest("Not a valid user");
            }
        }

    }
}