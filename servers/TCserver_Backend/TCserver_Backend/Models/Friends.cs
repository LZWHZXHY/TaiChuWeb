using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("friends")]
    public class Friends
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 确保Id自增
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // 主体用户ID
        [Required]
        public int? FriendId { get; set; }  // 好友ID（允许为空）

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public int? status { get; set; } = 0;  // 状态：0-正常，1-已删除

        [MaxLength(100)]
        public string Remark { get; set; } = "";

        // 导航属性
        [ForeignKey("UserId")]
        public virtual userdata User { get; set; }

        [ForeignKey("FriendId")]
        public virtual userdata Friend { get; set; }

    }
}
