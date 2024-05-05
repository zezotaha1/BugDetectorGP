using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.blog;
using BugDetectorGP.Models.user;
using BugDetectorGP.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
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
     
            var NewBlog = new Blogs();
            NewBlog.Title = model.Title;
            NewBlog.Content = model.Content;
            NewBlog.PublicationDate = DateTime.UtcNow.ToLocalTime();
            var findUser =  await _userManager.FindByNameAsync(userinfo.UserName);
            NewBlog.UserId=findUser.Id;
            _Context.Blogs.Add(NewBlog);
            _Context.SaveChanges();
            return Ok("New Blog are added");
        }

        [HttpGet("ReturnAllBlogs")]
        public async Task<IActionResult> ReturnAllBlogs()
        {
           var listBlog=_Context.Blogs.ToList();
            AuthModel userinfo = await GetUserProfile();
            
            var AllBlogs = new List<AllBlogsDTO>();
            foreach(var  blog in listBlog)
               {
                var UserData =await _Context.Users.SingleOrDefaultAsync(u => u.Id == blog.UserId);
                var temp = new AllBlogsDTO
                { 
                    Title = blog.Title,
                    Content = blog.Content,            
                    UsrName = UserData.UserName,
                    PublicationDate=blog.PublicationDate,
                    NumberOfDisLikes=blog.DislikeNumber,
                    NumberOfLikes=blog.LikeNumber
                };
                     AllBlogs.Add(temp);
               }
            return Ok(AllBlogs);
        }

    }
}
