using System.Reflection.Metadata;

namespace BugDetectorGP.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        public string Content { get; set; } = null!;

        public DateTime PublicationDate { get; set; }

        public int BlogId { get; set; }

        public string UserId { get; set; }

        public virtual Blogs Blog { get; set; } = null!;

        public virtual UserInfo User { get; set; } = null!;
    }
}
