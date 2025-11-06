namespace TCserver_Backend.Dtos
{
    public class ViewCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
        public int? ParentCommentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? UserAvatar { get; set; }
        // ...如有子评论
        public List<ViewCommentDto> Replies { get; set; }
    }
}
