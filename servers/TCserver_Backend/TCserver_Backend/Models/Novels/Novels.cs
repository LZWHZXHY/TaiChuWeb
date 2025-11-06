using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Novels
{
    [Table("novels")]
    public class Novels
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string desc { get; set; }
        [Required]
        public int type { get; set; } // 0:原创 1:转载
        [Required]
        public string series { get; set; } // 0:单行本 1:系列作
       
        public string? part { get; set; } // 分部
        [Required]
        public string author { get; set; }
        [Required]
        public int status { get; set; } // 0:连载中 1:已完结 2:已废弃
        [Required]
        public string cover_url { get; set; } // 封面URL
        [Required]
        public DateTime create_time { get; set; }
 
        public DateTime? update_time { get; set; }

        public int? likes { get; set; } // 点赞数

        public int? reports { get; set; } // 举报数
    }
}
