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

namespace BugDetectorGP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<UserInfo> _userManager;
        private Dictionary<string, long> EmailAndOTP=new Dictionary<string, long> { };

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
            if (EmailAndOTP.TryGetValue(model.email, out x)!)
            {
                return BadRequest("This email have OTP");
            }
            
            int otp = new Random().Next(100000, 999999);

            // Set the sender's email address and password
            string senderEmail = "bugdetector8@gmail.com";
            string senderPassword = "Z.z123456";

            using var message = new MailMessage(senderEmail, model.email)
            {
                // Set the subject and body of the email with the OTP
                Subject = "OTP for verification",
                Body = "Your OTP is: " + otp
            };

            // Create a new SmtpClient object
            using var client = new SmtpClient("smtp.gmail.com")
            {
                // Set the port and credentials for the SMTP server
                Port = 587, // Port number depends on your SMTP server configuration
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true // Enable SSL/TLS encryption
            };

            try
            {
                // Send the email
                client.Send(message);
                EmailAndOTP.Add(model.email, otp);
                return Ok("OTP sent successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest("Failed to send OTP: " + ex.Message);
            }
            finally
            {
                // Dispose of the MailMessage and SmtpClient objects
                message.Dispose();
                client.Dispose();
            }

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUser model)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            
            long x = -1;

            if (EmailAndOTP.TryGetValue(model.Email,out x)!)
            {
                return BadRequest("This email dosn\'t have OTP ");
            }

            var _EmailAndOTP = EmailAndOTP[model.Email];
            if (_EmailAndOTP != model.OTP)
            {
                return BadRequest("the OTP incorrect");
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
