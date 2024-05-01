namespace BugDetectorGP.Models
{
    public class Blogs
    {
        public int BlogId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime PublicationDate { get; set; }

        public int? CategoryId { get; set; }

        public string UserId { get; set; }

        public int LikeNumber { get; set; }

        public int DislikeNumber { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual UserInfo User { get; set; } = null!;
    }
}
