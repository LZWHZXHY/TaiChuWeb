using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.FeedBack
{
    [Table("feedback")]
    public class Feedback
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int type { get; set; }
        [Required]
        public string content { get; set; }
        [Required]
        public int status { get; set; }
        [Required]
        public DateTime createTime { get; set; }

        [Required]
        public string title { get; set; }

        public string? imagesUrl { get; set; }

        public int? contactQQ { get; set; }
    }
}
