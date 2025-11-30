using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace THCY_BE.Dto.FeedBack
{
    // 提交反馈的请求DTO
    public class FeedBackDto
    {
        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(50, ErrorMessage = "标题不能超过50个字符")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "内容不能为空")]
        [StringLength(1000, ErrorMessage = "内容不能超过1000个字符")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "请选择反馈类型")]
        [Range(1, 4, ErrorMessage = "请选择有效的反馈类型")]
        public int Type { get; set; }

        [StringLength(15, ErrorMessage = "QQ号码格式不正确")]
        [RegularExpression(@"^[1-9][0-9]{4,14}$", ErrorMessage = "QQ号码格式不正确")]
        public string? ContactQQ { get; set; }

        public IFormFile? ErrorImage { get; set; }
    }

    
}