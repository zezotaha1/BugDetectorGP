using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Dto
{
    public class CommentIdDTO
    {
        [Required]
        public int CommentId { get; set; }
    }
}
