using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("useractivity")]
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int ActivityType { get; set; } // 1=发帖, 2=评论, 3=点赞, 4=签到, 5=上传视频...
        [Required]
        public int Score { get; set; } // 行为获得的分数
        [Required]
        public DateTime CreateTime { get; set; }
    }
}