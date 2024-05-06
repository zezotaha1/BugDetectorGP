using BugDetectorGP.Dto;
using BugDetectorGP.Models;
using BugDetectorGP.Models.user;
using BugDetectorGP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Text;
using static System.Net.WebRequestMethods;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<UserInfo> _userManager;
        private Dictionary<string, long> EmailAndOTP=new Dictionary<string, long> { };
        private Dictionary<string, long> ForgotPasswordAndOTP = new Dictionary<string, long> { };

        public AccountController(IAuthService authService, UserManager<UserInfo> _userManager)
        {
            _authService = authService;
            this._userManager = _userManager;

        }
        [HttpPost("GenerateAnOTP")]
        public async Task<IActionResult> GenerateAnOTP(GenerateAnOTPDto model)
        {
            if(model.email == null)
                return BadRequest("Email is Require");

            if (await _userManager.FindByEmailAsync(model.email) is not null)
                return BadRequest("Email is already registered!") ;
            
            long x = -1;
            if (EmailAndOTP.ContainsKey(model.email)==true)
            {
                return Ok("This email have OTP");
            }
            
            int otp = new Random().Next(100000, 999999);

            string SendEmailStatus =await SendGridEmailSender.SendEmail(model.email, "OTP for Register verification", "Your OTP is: "+otp);


            if (SendEmailStatus != "Email sent successfully.") 
            {
                return BadRequest("Failed to send OTP: "+SendEmailStatus);
            }

            EmailAndOTP.Add(model.email, otp);
            return Ok("OTP and "+SendEmailStatus);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);

            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return BadRequest("Email is already registered!");

            if (EmailAndOTP.ContainsKey(model.Email)==false)
            {
                return BadRequest("This email dosn\'t have OTP. ");
            }

            if (EmailAndOTP[model.Email] != model.OTP)
            {
               
                return BadRequest("the OTP incorrect.");
            }
            
            var result = await _authService.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.message);

            SetRefreshTokenInCooke(result.RefreshToken, result.RefreshTokenExpiration);
            EmailAndOTP.Remove(model.Email);
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

            return Ok(result);
        }

        [Authorize]
        [HttpGet("LogOut")]
        public async Task<IActionResult>LogOut()
        {
            var token =Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token Is Require or you must be login");
            
            var resault=await _authService.LogoutAsync(token);
            
            if(!resault)
                return BadRequest("Token is Invalid");
            
            Response.Cookies.Delete("refreshToken");
            return Ok("Logout successful");
        }

        /*[HttpPost(" GenerateAnOTPForForgotPassword")]
        public async Task<IActionResult> GenerateAnOTPForPassword(GenerateAnOTPDto model)
        {
            if (model.email == null)
                return BadRequest("Email is Require");

            if (await _userManager.FindByEmailAsync(model.email) is null)
                return BadRequest("Email is not registered !");

            long x = -1;
            if (ForgotPasswordAndOTP.TryGetValue(model.email, out x)!)
            {
                return Ok("This email have OTP");
            }

            int otp = new Random().Next(100000, 999999);

            string SendEmailStatus = SendGridEmailSender.SendEmail(model.email, otp);

            if (SendEmailStatus != Task<"Email sent successfully.">)
            {
                return BadRequest("Failed to send OTP :"+SendEmailStatus);
            }

            ForgotPasswordAndOTP.Add(model.email, otp);
            return Ok(sendemail);
        }*/
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
