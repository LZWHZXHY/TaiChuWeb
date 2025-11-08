namespace TCserver_Backend.Models
{
    public class CommentReport
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string Reason { get; set; }
        public DateTime CreateTime { get; set; }
    }

}
