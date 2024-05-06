using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class BlogLikeAndDisLikeDTO
    {
        [Required]
        public int Blogid { get; set; }
    }
}
