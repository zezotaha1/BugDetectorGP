using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;

        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var result = await _authService.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.message);

            SetRefreshTokenInCooke(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            var result = await _authService.LoginAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.message);
            if(!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCooke(result.RefreshToken,result.RefreshTokenExpiration);
            
            Response.Cookies.Append("Token", result.Token);
            Response.Cookies.Append("refreshToken", result.RefreshToken);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("LogOut")]
        public async Task<IActionResult>LogOut([FromBody] LogOutUser model)
        {
            var token =Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token Is Require or you must be login");
            var resault=await _authService.LogoutAsync(token);
            if(!resault)
                return BadRequest("Token is Invalid");
            Response.Cookies.Delete("refreshToken");
            Response.Cookies.Delete("Token");
            return Ok("Logout successful");
        }
        private void SetRefreshTokenInCooke(string refreshtoken,DateTime expire)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expire.ToLocalTime(),
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("refreshToken", refreshtoken, cookieOptions);
        }
       
    }
}
