using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.BYDlibrary
{
    [Table("setting")]
    public class Setting
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string type { get; set; } 
        [Required]
        public string name { get; set; }

        public int? parent_id { get; set; } // 父级ID，0表示无父级

        public string? summary { get; set; } // 简介
        public string? image_url { get; set; } // 图片URL

        public string? extra { get; set; }

        public DateTime createTime { get; set; }
        public DateTime updateTime { get; set; }
    }
}
