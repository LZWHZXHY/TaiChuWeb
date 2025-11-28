using System.ComponentModel.DataAnnotations;

namespace THCY_BE.Dto.Friends
{
    public class SendFriendRequestDto
    {
        [Required(ErrorMessage = "目标用户ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "用户ID必须大于0")]
        public int ToUserId { get; set; }

        [MaxLength(100, ErrorMessage = "验证消息不能超过100个字符")]
        public string Remark { get; set; } = "";
    }

    public class SendFriendRequestResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int? RequestId { get; set; }
    }


}
