using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Models.Notification
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // 接收用户ID

        [Required]
        public int type { get; set; }    // 通知类型

        [Required]
        [MaxLength(200)]
        public string title { get; set; } = string.Empty;  // 通知标题

        [Required]
        public string content { get; set; } = string.Empty; // 通知内容

        [Required]
        public int senderID { get; set; } = 1;  // 发送者ID（系统通知为1）

        [Required]
        public bool IsRead { get; set; } = false;  // 是否已读

        [Required]
        public bool IsDeleted { get; set; } = false;  // 是否删除

        public DateTime? ExpireTime { get; set; }  // 过期时间

        public DateTime CreateTime { get; set; } = DateTime.Now;  // 创建时间

        public DateTime? ReadTime { get; set; }  // 阅读时间

        // 导航属性
        [ForeignKey("UserId")]
        public virtual UserData user { get; set; }

        [ForeignKey("senderID")]
        public virtual UserData Sender { get; set; }

 
    }
}
