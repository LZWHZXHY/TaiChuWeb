using System.ComponentModel.DataAnnotations;
namespace THCY_BE.Dto.Notification
{
    public class NotificationRequest
    {
        [Required]
        public int Type { get; set; }

        public string? Title { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public List<int> Recipients { get; set; } = new();
        public int? SenderId { get; set; }
        public DateTime? ExpireTime { get; set; }

    }
}
