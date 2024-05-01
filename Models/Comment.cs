using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace BugDetectorGP.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public string Content { get; set; }

        public DateTime PublicationDate { get; set; }

        public int BlogId { get; set; }

        public string UserId { get; set; }

        public virtual Blogs Blog { get; set; } 

        public virtual UserInfo User { get; set; } 
    }
}
