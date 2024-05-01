﻿using System.ComponentModel.DataAnnotations;

namespace BugDetectorGP.Models
{
    public class Blogs
    {
        [Key]
        public int BlogId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime PublicationDate { get; set; } = DateTime.Now;

        public string UserId { get; set; }

        public int LikeNumber { get; set; }

        public int DislikeNumber { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual UserInfo User { get; set; }
    }
}
