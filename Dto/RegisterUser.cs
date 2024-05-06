using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class RegisterUser
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public int OTP { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
