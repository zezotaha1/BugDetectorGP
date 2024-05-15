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
   // [Authorize]
    public class BlogsController : ControllerBase
    {
        private static ProfileDataController _ProfileData = new ProfileDataController();
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
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            AuthModel userinfo = await _ProfileData.GetUserProfile();
     
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
        public async Task<IActionResult> DeleteBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await _ProfileData.GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
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
        [HttpPost("ReturnAllBlogs")]
        public async Task<IActionResult> ReturnAllBlogs()
        {
            var listBlog=_Context.Blogs.ToList();
            AuthModel userinfo = await _ProfileData.GetUserProfile();
            
            var AllBlogs = new List<AllBlogsDTO>();
            foreach(var  blog in listBlog)
            {
                var UserData =await _Context.Users.SingleOrDefaultAsync(u => u.Id == blog.UserId);
                var temp = new AllBlogsDTO
                {
                    Id=blog.BlogId,
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
        [HttpPost("Like")]
        public async Task<IActionResult>LikeToBlog(BlogIdDTO model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await _ProfileData.GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            
            if (blog == null)
                return BadRequest("Blog Not Found");
           
            var findBlogLike=await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => ( b.BlogId == model.BlogId && b.UserId == findUser.Id ));
            
            if(findBlogLike==null )
            {
                var LikeBlog = new LikesAndDislikes();
                LikeBlog.PublicationDate = DateTime.Now.ToLocalTime();
                LikeBlog.LikeOrDislike = true;
                LikeBlog.UserId = findUser.Id;
                LikeBlog.BlogId=model.BlogId;
                blog.LikeNumber+=1;
                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();
                return Ok("Like added");
            }
            if(findBlogLike.LikeOrDislike==false)
            {
                findBlogLike.LikeOrDislike = true;
                blog.LikeNumber += 1;
                blog.DislikeNumber -= 1;
                blog.DislikeNumber = int.Max(0, blog.DislikeNumber);
                _Context.SaveChanges();
                return Ok("Like added");
            }
            blog.LikeNumber -= 1;
            blog.LikeNumber = int.Max(0, blog.LikeNumber);
            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();
            return Ok("Your Like removed");
        }

        [HttpPost("DisLike")]
        public async Task<IActionResult> DisLikeToBlog(BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AuthModel userinfo = await _ProfileData.GetUserProfile();
            var findUser = await _userManager.FindByNameAsync(userinfo.UserName);
            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            if (blog == null)
                return BadRequest("Blog Not Found");
            var findBlogLike = await _Context.LikesAndDislikes.SingleOrDefaultAsync(b => (b.BlogId == model.BlogId && b.UserId == findUser.Id));
            if (findBlogLike == null)
            {
                var LikeBlog = new LikesAndDislikes();
                LikeBlog.PublicationDate = DateTime.Now.ToLocalTime();
                LikeBlog.LikeOrDislike = false;
                LikeBlog.UserId = findUser.Id;
                LikeBlog.BlogId = model.BlogId;
                blog.DislikeNumber+= 1;
                _Context.LikesAndDislikes.Add(LikeBlog);
                _Context.SaveChanges();
                return Ok("Dislike added");
            }
            if (findBlogLike.LikeOrDislike == true)
            {
                findBlogLike.LikeOrDislike = false;
                blog.DislikeNumber += 1;
                blog.LikeNumber -= 1;
                blog.LikeNumber=int.Max(0,blog.LikeNumber);
                _Context.SaveChanges();
                return Ok("Dislike added");
            }
            blog.DislikeNumber -= 1;
            blog.DislikeNumber=int.Max(0,blog.DislikeNumber) ;
            _Context.LikesAndDislikes.Remove(findBlogLike);
            _Context.SaveChanges();
            return Ok("Your DisLike removed");
        }

        [HttpPost("ReturnOneBlog")]
        public async Task<IActionResult> ReturnOneBlog (BlogIdDTO model)
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


        [HttpPost("Search")]
        public async Task<IActionResult>SearchInBlogs(SearchDTO model)
        {

            return Ok();
        }
    }
}
