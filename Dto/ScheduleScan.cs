using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class ScheduleScan
    {

        [Required]
        public string url { get; set; }
        
        [Required]
        public DateTime date { get; set; }
    }
}
