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

        [HttpPost("AddBlog")]
        public async Task<IActionResult> AddBlog(BlogModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var NewBlog = new Blogs
            {
                Title = model.Title,
                Content = model.Content,
                PublicationDate = DateTime.UtcNow.ToLocalTime(),
                UserId = findUser.Id
            };
            _Context.Blogs.Add(NewBlog);
            _Context.SaveChanges();
            return Ok("New Blog is added");
        }
        [Authorize]
        [HttpDelete("DeleteBlog")]
        public async Task<IActionResult> DeleteBlog(BlogIdDTO model)
        {
           
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            if (blog == null)
                return BadRequest("Blog Not Found");

            if (blog.UserId == findUser.Id)
            {
                _Context.Blogs.Remove(blog);
                _Context.SaveChanges();
                return Ok("Blog is Deleted");
            }
            return BadRequest("You do not have Access to remove this Blog");
        }

        [HttpPost("ReturnAllBlogs")]
        [AllowAnonymous]
        public async Task<IActionResult> ReturnAllBlogs()
        {
            var listBlog = _Context.Blogs.ToList();
            var AllBlogs = new List<AllBlogsDTO>();
            foreach (var blog in listBlog)
            {
                var UserData = await _Context.Users.SingleOrDefaultAsync(u => u.Id == blog.UserId);
                var temp = new AllBlogsDTO
                {
                    Id = blog.BlogId,
                    Title = blog.Title,
                    Content = blog.Content,
                    UsrName = UserData.UserName,
                    PublicationDate = blog.PublicationDate,
                    NumberOfDisLikes = blog.DislikeNumber,
                    NumberOfLikes = blog.LikeNumber
                };
                AllBlogs.Add(temp);
            }
            return Ok(AllBlogs);
        }

        [HttpPost("ReturnOneBlog")]
        public async Task<IActionResult> ReturnOneBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            if (blog == null)
                return BadRequest("Blog Not Found");

            var UserData = await _Context.Users.SingleOrDefaultAsync(u => u.Id == blog.UserId);
            var temp = new AllBlogsDTO
            {
                Id = blog.BlogId,
                Title = blog.Title,
                Content = blog.Content,
                UsrName = UserData.UserName,
                PublicationDate = blog.PublicationDate,
                NumberOfDisLikes = blog.DislikeNumber,
                NumberOfLikes = blog.LikeNumber
            };

            return Ok(temp);
        }

        [HttpPost("Like")]
        public async Task<IActionResult> LikeToBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);

            if (blog == null)
                return BadRequest("Blog Not Found");

            var findBlogLike = await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => (b.BlogId == model.BlogId && b.UserId == findUser.Id));

            if (findBlogLike == null)
            {
                var LikeBlog = new LikesAndDislikes
                {
                    PublicationDate = DateTime.Now.ToLocalTime(),
                    LikeOrDislike = true,
                    UserId = findUser.Id,
                    BlogId = model.BlogId
                };

                blog.LikeNumber++;

                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();

                return Ok("Like added");
            }
            if (findBlogLike.LikeOrDislike == false)
            {
                findBlogLike.LikeOrDislike = true;

                blog.LikeNumber++;
                blog.DislikeNumber--;

                _Context.SaveChanges();

                return Ok("Like added and your Dislike removed");
            }

            blog.LikeNumber--;

            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();

            return Ok("Your Like removed");
        }

        [HttpPost("DisLike")]
        public async Task<IActionResult> DisLikeToBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);

            if (blog == null)
                return BadRequest("Blog Not Found");
            var findBlogLike = await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => (b.BlogId == model.BlogId && b.UserId == findUser.Id));
            if (findBlogLike == null)
            {
                var LikeBlog = new LikesAndDislikes
                {
                    PublicationDate = DateTime.Now.ToLocalTime(),
                    LikeOrDislike = false,
                    UserId = findUser.Id,
                    BlogId = model.BlogId
                };

                blog.DislikeNumber++;

                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();

                return Ok("Dislike added");
            }
            if (findBlogLike.LikeOrDislike == true)
            {
                findBlogLike.LikeOrDislike = false;

                blog.DislikeNumber++;
                blog.LikeNumber--;

                _Context.SaveChanges();

                return Ok("Dislike added and your like removed");
            }

            blog.DislikeNumber--;

            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();

            return Ok("Your DisLike removed");
        }

    }
}
