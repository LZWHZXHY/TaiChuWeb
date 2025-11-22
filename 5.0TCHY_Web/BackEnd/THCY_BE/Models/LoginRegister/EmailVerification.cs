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
        [Required]
        public DateTime ExpireTime { get; set; }
        [Required]
        public DateTime createTime { get; set; } = DateTime.UtcNow;

        [Required]
        public int purpose { get; set; } //1 表示用于注册账号的验证，2表示用于密码找回的验证

        [Required]
        public int used {  get; set; } //0 表示未被使用， 1表示已经被使用了。
    }
}
