using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class BlogIdDTO
    {
        [Required]
        public int BlogId { get; set; }
    }
}
