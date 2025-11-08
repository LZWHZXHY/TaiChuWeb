using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Dtos
{
    public class RegisterWithCodeDto
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "用户名长度需在3-50个字符之间")]
        public string username { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱地址不能为空")]
        [EmailAddress(ErrorMessage = "请输入有效的邮箱地址")]
        public string email_address { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度需在6-100个字符之间")]
        public string password_hash { get; set; } = string.Empty;

        [Required(ErrorMessage = "验证码不能为空")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "请输入6位验证码")]
        public string verification_code { get; set; } = string.Empty;
    }
}