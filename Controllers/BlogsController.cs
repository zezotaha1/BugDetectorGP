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
        [HttpDelete("DeleteBlog")]
        public async Task<IActionResult> DeleteBlog(BlogLikeAndDisLikeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.Blogid);
            if (blog == null)
                return BadRequest("Blog Not Found");
            if(blog.UserId == findUser.Id)
            {
                _Context.Blogs.Remove(blog);
                _Context.SaveChanges();
                return Ok("Blog are Deleted");
            }
            return BadRequest("You dont Acsess this Blog");
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
        [HttpGet("Like")]
        public async Task<IActionResult>LikeToBlog(BlogLikeAndDisLikeDTO model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.Blogid);
            if (blog == null)
                return BadRequest("Blog Not Found");
            var findBlogLike=await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => ( b.BlogId == model.Blogid && b.UserId == findUser.Id ));
            if(findBlogLike==null )
            {
                var LikeBlog = new LikesAndDislikes();
                LikeBlog.PublicationDate = DateTime.Now.ToLocalTime();
                LikeBlog.LikeOrDislike = true;
                LikeBlog.UserId = findUser.Id;
                LikeBlog.BlogId=model.Blogid;
                blog.LikeNumber+=1;
                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();
                return Ok();
            }
            if(findBlogLike.LikeOrDislike==false)
            {
                findBlogLike.LikeOrDislike = true;
                blog.LikeNumber += 1;
                blog.DislikeNumber -= 1;
                _Context.SaveChanges();
                return Ok("Like added");
            }
            blog.LikeNumber -= 1;
            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();
            return Ok("Your Like removed");

        }
        [HttpGet("DisLike")]
        public async Task<IActionResult> DisLikeToBlog(BlogLikeAndDisLikeDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.Blogid);
            if (blog == null)
                return BadRequest("Blog Not Found");
            var findBlogLike = await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => (b.BlogId == model.Blogid && b.UserId == findUser.Id));
            if (findBlogLike == null)
            {
                var LikeBlog = new LikesAndDislikes();
                LikeBlog.PublicationDate = DateTime.Now.ToLocalTime();
                LikeBlog.LikeOrDislike = false;
                LikeBlog.UserId = findUser.Id;
                LikeBlog.BlogId = model.Blogid;
                blog.DislikeNumber+= 1;
                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();
                return Ok();
            }
            if (findBlogLike.LikeOrDislike == true)
            {
                findBlogLike.LikeOrDislike = false;
                blog.DislikeNumber += 1;
                blog.LikeNumber -= 1;
                _Context.SaveChanges();
                return Ok("Dislike added");
            }
            blog.DislikeNumber -= 1;
            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();
            return Ok("Your Like removed");
        }
        [HttpPost("Comment")]
        public async Task<IActionResult> AddCommentToBlog(CommentDto model)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            if (blog == null)
                return BadRequest("Blog Not Found");
            var addcomment = new Comment();
            addcomment.PublicationDate = DateTime.Now.ToLocalTime();
            addcomment.Content = model.Comment;
            addcomment.UserId = findUser.Id;
            addcomment.BlogId = model.BlogId;
            _Context.Comment.Add(addcomment);
            _Context.SaveChanges();
            return Ok(addcomment);
        }
        [HttpDelete("DeleteComment")]
        public async Task<IActionResult>DeleteComment(DeleteCommentDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            var comment=await _Context.Comment.SingleOrDefaultAsync(c=>c.CommentId == model.CommentId);
            if (blog == null||comment==null)
                return BadRequest("Blog or Comment Not Found");
            if (comment.UserId != findUser.Id)
                return BadRequest("you Dont Remove this Comment");
            _Context.Comment.Remove(comment);
            _Context.SaveChanges();
            return Ok("Comment are Deleted");
        }
    }
}
