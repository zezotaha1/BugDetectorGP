using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.blog;
using BugDetectorGP.Models.user;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        public AdminController(ApplicationDbContext Context)
        {
            _Context = Context;
        }
        [HttpPost("AddBlog")]
        public async Task<IActionResult> AddBlog(BlogModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var NewBlog = new Blogs();
            NewBlog.Title = model.Title;
            NewBlog.Content = model.Content;
            NewBlog.PublicationDate = DateTime.UtcNow.ToLocalTime();
            NewBlog.UserId = "1";
            _Context.Blogs.Add(NewBlog);
            _Context.SaveChanges();
            return Ok("New Blog a");
         }
        [HttpDelete("DeleteBlog")]
        public async Task<IActionResult> DeleteBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            if (blog == null)
                return BadRequest("Blog Not Found");
         
                _Context.Blogs.Remove(blog);
                _Context.SaveChanges();
                return Ok("Blog are Deleted");
        }
        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment(DeleteCommentDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            var comment = await _Context.Comment.SingleOrDefaultAsync(c => c.CommentId == model.CommentId);
            if (blog == null || comment == null)
                return BadRequest("Blog or Comment Not Found");
            _Context.Comment.Remove(comment);
            _Context.SaveChanges();
            return Ok("Comment are Deleted");
        }










        [HttpGet("isAdmin")]
        public IActionResult IsAdmin()
        {
            var isAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            return Ok(isAdmin);
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
                Email = userEmail,
                IsAuthenticated = true
            };
        }

    }
}
