using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class ForgotPasswordDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public int OTP { get; set; }
        [Required]
        [RegularExpression("(?=^.{6,10}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\\s).*$")]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
