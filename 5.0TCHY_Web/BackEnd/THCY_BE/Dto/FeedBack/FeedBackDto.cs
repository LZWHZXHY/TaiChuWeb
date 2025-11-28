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

    // 通用的API响应DTO
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    // 反馈项DTO（用于列表和详情）
    public class FeedBackItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; }
        public string TypeName => GetTypeName(Type);
        public int Status { get; set; }
        public string StatusName => GetStatusName(Status);
        public DateTime CreateTime { get; set; }

        private static string GetTypeName(int type) => type switch
        {
            1 => "网站BUG反馈",
            2 => "社区意见",
            3 => "内容举报",
            4 => "其他",
            _ => "未知类型"
        };

        private static string GetStatusName(int status) => status switch
        {
            0 => "待处理",
            1 => "处理中",
            2 => "已解决",
            3 => "已关闭",
            _ => "未知状态"
        };
    }

    // 分页信息DTO
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // 更新状态DTO
    public class UpdateStatusDto
    {
        [Required]
        [Range(0, 3)]
        public int Status { get; set; }
    }
}