using AuthenticationPlugin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestAPI1._0.Data;
using RestAPI1._0.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestAPI1._0.Controllers
{
    //Added [action] for preventing need to write [action] in every method 
    [Route("api/[controller]/[action]")]
    [ApiController]
    //Create database bridgewith _dbContext, then 
    public class UsersController : ControllerBase
    {
        private CinemaDbContext _dbContext;

        //Using configuration extension, and Auth library 
        private IConfiguration _configuration;
        private readonly AuthService _auth;
        public UsersController(CinemaDbContext dbContext, IConfiguration configuration)
        {
             _dbContext = dbContext;
            _configuration = configuration;
            _auth = new AuthService(_configuration);
        }

        //Register user by checking if there is already the same email in the database.
        //Assign Role in backend for security 
        [HttpPost]
        public IActionResult Register([FromBody ] User user)
        {
            //Look inside the database where the email in the database is equal to the incoming user email 
            //then use SingleOrDefault returns only single element if a match is found, and another if not found
            var userWithSameEmail = _dbContext.Users.Where(u => u.Email == user.Email).SingleOrDefault();
            if(userWithSameEmail != null)
            {
                return BadRequest("Email already exists");
            }
            var userObj = new User
            {
                Name = user.Name,
                Email = user.Email,
                //From the Authentication Nuget Package to help hash passwords
                Password = SecurePasswordHasherHelper.Hash(user.Password),
                Role = "Users"
            };
            _dbContext.Users.Add(userObj);
            _dbContext.SaveChanges();
            return StatusCode(StatusCodes.Status201Created);
        }
        [HttpPost]
        public IActionResult Login([FromBody]User user)
        {
            var userEmail = _dbContext.Users.FirstOrDefault(u => u.Email == user.Email);
            if(userEmail == null)
            {
                return NotFound();
            }

            //BEGIN SECURITY PROCESS-------------
            //If password doesn't match hash, deny authentication 
            if (!SecurePasswordHasherHelper.Verify(user.Password, userEmail.Password))
            {
                return Unauthorized();
            }
            //Password Accepted
            //Register claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, userEmail.Role)
            };
            //Then pass the claims in the GenerateAccessToken method to get the access token.
            var token = _auth.GenerateAccessToken(claims);
            //For the token to be in JSON
            return new ObjectResult(new
            {
                access_token = token.AccessToken,
                expires_in = token.ExpiresIn,
                token_type = token.TokenType,
                creation_Time = token.ValidFrom,
                expiration_Time = token.ValidTo,
                //return user id
                user_id = userEmail.Id
            });
        }
    }
}
