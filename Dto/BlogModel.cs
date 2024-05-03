using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class BlogModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
