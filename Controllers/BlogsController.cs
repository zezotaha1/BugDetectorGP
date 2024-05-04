using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BlogsController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<UserInfo> _userManager;

        public BlogsController(ApplicationDbContext Context, UserManager<UserInfo> userManager)
        {
            _Context = Context;
            _userManager = userManager;
        }
        [HttpGet("current-token")]
        public IActionResult GetCurrentToken()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                var tokenParts = authorizationHeader.Split(" ");

                if (tokenParts.Length == 2 && tokenParts[0].Equals("Bearer"))
                {
                    return Ok(tokenParts[1]);
                }
            }

            return BadRequest("Token not found in the authorization header.");
        }
        [HttpGet("profile")]
        public async Task<AuthModel> GetUserProfile()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(userName))
            {
                return new AuthModel
                {
                    message = "User ID not found in token.",
                    IsAuthenticated = false
                    
                };
            }
            return new AuthModel
            {
                UserName = userName,
                Email = userEmail
            };
        }

        [HttpGet("isAdmin")]
        public IActionResult IsAdmin()
        {
            var isAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");

            return Ok(isAdmin);
        }
        [HttpPost("AddBlog")]
        public async Task<IActionResult> AddBlog(BlogModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var token = GetCurrentToken();
            AuthModel userinfo = await GetUserProfile();
           /* if(userinfo.IsAuthenticated==false) {
               return Unauthorized();
            }*/
           
            var NewBlog = new Blogs();
            NewBlog.Title = model.Title;
            NewBlog.Content = model.Content;
            NewBlog.PublicationDate = DateTime.UtcNow;
            var findUser =  await _userManager.FindByNameAsync(userinfo.UserName);
            NewBlog.UserId=findUser.Id;
            _Context.Add(NewBlog);
            _Context.SaveChanges();
            return Ok("New Blog are added");
        }
        

    }
}
