using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Models
{
    // 举报表
    public class PostReport
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PostId { get; set; }
        [Required]
        public int UserId { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }
        [Required]
        public DateTime CreateTime { get; set; }
    }
}
