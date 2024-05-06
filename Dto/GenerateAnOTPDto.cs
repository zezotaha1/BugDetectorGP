using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class GenerateAnOTPDto
    {
        [Required]
        public string email { get; set; }
    }
}
