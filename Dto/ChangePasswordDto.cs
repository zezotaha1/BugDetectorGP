using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }
       
        [Required]
        [RegularExpression("(?=^.{6,10}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\\s).*$")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; }

    }
}
