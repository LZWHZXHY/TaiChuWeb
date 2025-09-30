using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("comments")]
    public class Comments
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public int post_id { get; set; } //评论所属帖子ID
        [Required]
        public int user_id { get; set; } //评论用户ID
        [Required]
        public string content { get; set; } //评论内容
        public DateTime createTime { get; set; } //评论创建时间
        public int? ParentCommentId { get; set; } // 父评论ID，0表示顶级评论
        [Required]
        public int status { get; set; } //评论状态，0为正常，1为删除，2为封禁

        public int report_count { get; set; } //评论举报数

        [ForeignKey("post_id")]
        public virtual Posts Posts { get; set; }

        [ForeignKey("user_id")]
        public virtual useraccount useraccount { get; set; }
    }
}
