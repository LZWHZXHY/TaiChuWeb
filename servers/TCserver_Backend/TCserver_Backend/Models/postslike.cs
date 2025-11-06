using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("postslike")]
    public class PostsLike
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int PostId { get; set; } //帖子ID
        [Required]
        public int UserId { get; set; } //用户ID
        [Required]
        public DateTime CreateTime { get; set; } //点赞时间
    }
}
