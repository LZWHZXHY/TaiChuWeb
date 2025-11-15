using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.LoginRegister
{
    [Table("emailverification")]
    public class EmailVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)] // 确保长度匹配数据库
        public string Email { get; set; } = string.Empty; // 添加默认值

        [Required]
        [StringLength(6)] // 验证码长度
        public string Code { get; set; } = string.Empty; // 添加默认值

        public DateTime ExpireTime { get; set; }

        public DateTime createTime { get; set; } = DateTime.UtcNow;
    }
}
