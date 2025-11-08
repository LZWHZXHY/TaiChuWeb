using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("friend_requests")]
    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int FromUserId { get; set; }  // 发送方用户ID

        [Required]
        public int ToUserId { get; set; }    // 接收方用户ID

        [MaxLength(100)]
        public string Remark { get; set; } = "";  // 好友请求备注

        [Required]
        public int Status { get; set; } = 0;  // 状态：0-待处理，1-已同意，2-已拒绝

        [Required]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime? ProcessTime { get; set; }  // 处理时间

        // 导航属性
        [ForeignKey("FromUserId")]
        public virtual userdata FromUser { get; set; }

        [ForeignKey("ToUserId")]
        public virtual userdata ToUser { get; set; }
    }
}