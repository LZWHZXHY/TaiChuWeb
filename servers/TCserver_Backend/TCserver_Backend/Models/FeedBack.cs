using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("feedback")]
    public class FeedBack
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
    }
}
