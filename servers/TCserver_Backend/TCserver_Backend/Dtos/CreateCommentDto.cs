namespace TCserver_Backend.Dtos
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; }
        public int? ParentCommentId { get; set; } // 可选，楼中楼
    }
}
