using System.ComponentModel.DataAnnotations;
namespace BugDetectorGP.Dto
{
    public class NetworkScan
    {
        [Required]
        public string ip { get; set; }
    }
}
