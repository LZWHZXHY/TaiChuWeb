using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
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
        public virtual userdata User { get; set; }

        [ForeignKey("senderID")]
        public virtual userdata Sender { get; set; }

        // 计算属性
        [NotMapped]
        public string TypeName => NotificationTypes.GetTypeName(type);

        [NotMapped]
        public string TypeIcon => NotificationTypes.GetTypeIcon(type);

        [NotMapped]
        public bool IsExpired => ExpireTime.HasValue && ExpireTime < DateTime.Now;


        // 在 Notification 类中添加
        [NotMapped]
        public int? RelatedId { get; set; } // 关联ID（如好友请求ID）

        [NotMapped]
        public string ActionData { get; set; } // 动作数据（如好友请求的备注信息）
    }
}