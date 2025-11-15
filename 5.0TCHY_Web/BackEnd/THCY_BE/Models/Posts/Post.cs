using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Models.Posts
{
    [Table("posts")]
    public class Post
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string post_title { get; set; }
        [Required]
        public string content { get; set; }

        [Required]
        public int author_id { get; set; } //作者ID
        [Required]
        public int post_type { get; set; } //帖子类型，0为柴圈帖子，1为游戏帖子，2为xx帖子
        [Required]
        public DateTime createTime { get; set; } //帖子发布时间
        [Required]
        public DateTime updateTime { get; set; } //帖子最后更新时间
        [Required]
        public int status { get; set; } //帖子状态，0为正常，1为删除，2为封禁

        public int view_count { get; set; } //帖子浏览量
        public int like_count { get; set; } //帖子点赞数
        public int comment_count { get; set; } //帖子评论数
        public int report { get; set; } //帖子举报数

        [Column(TypeName = "text")]
        public string? images { get; set; } // 允许为null
        [Column(TypeName = "text")]
        public string? videos { get; set; } // 允许为null

        [ForeignKey("author_id")]
        public virtual UserAccount useraccount { get; set; }
        [ForeignKey("author_id")]
        public virtual UserData userdata { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
