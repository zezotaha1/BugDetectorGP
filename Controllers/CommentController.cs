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
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDbContext _Context;
        private readonly UserManager<UserInfo> _userManager;

        public CommentController(ApplicationDbContext Context , UserManager<UserInfo> userManager)
        {
            _Context = Context;
            _userManager = userManager;
        }

        [HttpPost("ReturnCommentsForBlog")]
        public async Task<IActionResult> ReturnCommentsForBlog (BlogIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var blog = await _Context.Blogs.SingleOrDefaultAsync(u => u.BlogId == model.BlogId);
            
            if (blog == null)
                return BadRequest("Blog Not Found");

            var listOfComments = _Context.Comment.ToList().Where(C=>C.BlogId==model.BlogId);
            var Comments = new List<CommentReturnDTO>();
            foreach (var comment in listOfComments) 
            {
                var UserData = await _Context.Users.SingleOrDefaultAsync(u => u.Id == comment.UserId);
                Comments.Add(new CommentReturnDTO {
                    Id=comment.CommentId,
                    Content=comment.Content,
                    PublicationDate=comment.PublicationDate,
                    UsrName=UserData.UserName,
                    NumberOfLikes=comment.LikeNumber,
                    NumberOfDisLikes=comment.DislikeNumber,
                });
            }
            return Ok(Comments);
        }

        [HttpPost("AddComment")]
        public async Task<IActionResult> AddCommentToBlog(CommentDTO model)
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

            var addcomment = new Comment();
            addcomment.PublicationDate = DateTime.Now.ToLocalTime();
            addcomment.Content = model.Comment;
            addcomment.UserId = findUser.Id;
            addcomment.BlogId = model.BlogId;
            _Context.Comment.Add(addcomment);
            _Context.SaveChanges();
            return Ok("New Comment is added");
        }

        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment(DeleteCommentDto model)
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
            var comment = await _Context.Comment.SingleOrDefaultAsync(c => c.CommentId == model.CommentId);

            if (blog == null || comment == null)
                return BadRequest("Blog or Comment Not Found");

            if (comment.UserId != findUser.Id)
                return BadRequest("You Dont Have Access remove this Comment");

            _Context.Comment.Remove(comment);
            _Context.SaveChanges();

            return Ok("Comment is Deleted");
        }

        [HttpPost("Like")]
        public async Task<IActionResult> LikeToBlog(CommentIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var Comment = await _Context.Comment.SingleOrDefaultAsync(u => u.CommentId == model.CommentId);

            if (Comment == null)
                return BadRequest("Comment Not Found");

            var findCommentLike = await _Context.LikesAndDislikesForComments.SingleOrDefaultAsync(C => (C.CommentId == model.CommentId && C.UserId == findUser.Id));

            if (findCommentLike == null)
            {
                var LikeComment = new LikesAndDislikesForComments();
                LikeComment.PublicationDate = DateTime.Now.ToLocalTime();
                LikeComment.LikeOrDislike = true;
                LikeComment.UserId = findUser.Id;
                LikeComment.CommentId = model.CommentId;

                Comment.LikeNumber++;

                _Context.LikesAndDislikesForComments.Add(LikeComment);
                _Context.SaveChanges();

                return Ok("Like added");
            }
            if (findCommentLike.LikeOrDislike == false)
            {
                findCommentLike.LikeOrDislike = true;

                Comment.LikeNumber++;
                Comment.DislikeNumber--;

                _Context.SaveChanges();

                return Ok("Like added and your Dislike removed");
            }

            Comment.LikeNumber--;
            _Context.LikesAndDislikesForComments.Remove(findCommentLike);
            _Context.SaveChanges();
            return Ok("Your Like removed");
        }

        [HttpPost("DisLike")]
        public async Task<IActionResult> DisLikeToBlog(CommentIdDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var findUser = await _userManager.FindByNameAsync(userName);

            if (findUser == null)
            {
                return BadRequest("You must be login or your username incorrect");
            }

            var Comment = await _Context.Comment.SingleOrDefaultAsync(u => u.CommentId == model.CommentId);

            if (Comment == null)
                return BadRequest("Comment Not Found");

            var findCommentLike = await _Context.LikesAndDislikesForComments.SingleOrDefaultAsync(C => (C.CommentId == model.CommentId && C.UserId == findUser.Id));

            if (findCommentLike == null)
            {
                var LikeComment = new LikesAndDislikesForComments();
                LikeComment.PublicationDate = DateTime.Now.ToLocalTime();
                LikeComment.LikeOrDislike = false;
                LikeComment.UserId = findUser.Id;
                LikeComment.CommentId = model.CommentId;

                Comment.DislikeNumber++;

                _Context.LikesAndDislikesForComments.Add(LikeComment);
                _Context.SaveChanges();

                return Ok("Dislike added");
            }
            if (findCommentLike.LikeOrDislike == true)
            {
                findCommentLike.LikeOrDislike = true;
                Comment.LikeNumber -= 1;
                Comment.DislikeNumber += 1;
                Comment.LikeNumber = int.Max(0, Comment.DislikeNumber);
                _Context.SaveChanges();

                return Ok("Dislike added and your like removed");
            }

            Comment.DislikeNumber--;

            _Context.LikesAndDislikesForComments.Remove(findCommentLike);
            _Context.SaveChanges();
            return Ok("Your Like removed");
        }
    }
}
