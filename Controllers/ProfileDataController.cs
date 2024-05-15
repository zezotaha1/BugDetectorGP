using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BugDetectorGP.Dto;
using BugDetectorGP.Models.user;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileDataController : ControllerBase
    {
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
    }
}
