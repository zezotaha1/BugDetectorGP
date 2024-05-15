namespace BugDetectorGP.Dto
{
    public class CommentReturnDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UsrName { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfDisLikes { get; set; }
        public DateTime PublicationDate { get; set; }

    }
}
