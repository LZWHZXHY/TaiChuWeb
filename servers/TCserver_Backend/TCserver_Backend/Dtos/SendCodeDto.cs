using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Dtos
{

    public class SendCodeDto
    {

        [Required(ErrorMessage = "邮箱地址不能为空")]
        [EmailAddress(ErrorMessage = "请提供有效的邮箱地址")]
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
        public string email_address { get; set; } = string.Empty; // 添加默认值

        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string username { get; set; } = string.Empty;
    }
}