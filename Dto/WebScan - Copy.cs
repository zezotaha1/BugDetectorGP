using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class WebScan
    {
        [Required]
        public string url { get; set; }
    }
}
