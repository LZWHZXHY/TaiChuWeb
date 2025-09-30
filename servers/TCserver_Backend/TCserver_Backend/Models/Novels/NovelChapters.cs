using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Novels
{
    [Table("novel_chapters")]
    public class NovelChapters
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int novel_id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string content { get; set; }

        [Required]
        public int order_num { get; set; } // 章节顺序
        [Required]
        public DateTime createTime { get; set; }

      

        public DateTime? updateTime { get; set; }

  
        public int? likes { get; set; } // 点赞数
      
        public int? reports { get; set; } // 举报数

        [Required]
        public string author { get; set; } // 章节作者，默认为小说作者

        public int? views { get; set; }

        [Required]
        public int status { get; set; } // 0:正常 1:已删除 2:审核中 3:审核未通过
    }
}
