using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Communication.RESTControllers
{
    class TokenTest
    {
        public static string GenerateToken()
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("My own test key yeah"));
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, "John"),
                new Claim(JwtRegisteredClaimNames.Email, "john.doe@blinkingcaret.com"),
                new Claim(JwtRegisteredClaimNames.Exp,
                    $"{new DateTimeOffset(DateTime.Now.Add(new TimeSpan(1))).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
            };

            var token = new JwtSecurityToken(new JwtHeader(new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)), new JwtPayload(claims));
            string result = new JwtSecurityTokenHandler().WriteToken(token);
            return result;
        }
    }

    [Produces("application/json")]
    [Route("api/Token")]
    public class TokenController : Controller
    {
        [HttpGet("{username}/{password}")]
        public IActionResult GetToken(string username, string password)
        {
            return new ObjectResult(TokenTest.GenerateToken());
        }
    }
}