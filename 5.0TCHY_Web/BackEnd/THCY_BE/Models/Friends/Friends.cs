using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using THCY_BE.Models.UserDate;

namespace THCY_BE.Models.Friends
{
    [Table("friends")]
    public class Friend
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
        public virtual UserData User { get; set; }

        [ForeignKey("FriendId")]
        public virtual UserData friend { get; set; }
    }
}
