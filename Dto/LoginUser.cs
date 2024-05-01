using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class LoginUser
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
