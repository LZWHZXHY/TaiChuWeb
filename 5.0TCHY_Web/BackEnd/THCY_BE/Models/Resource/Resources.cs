using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Resource
{
    [Table("resources")]
    public class Resources
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string title { get; set; }

        public string desc { get; set; }

        public string content { get; set; }

        [Required]
        public int category_id { get; set; }
        [Required]
        public string download_url { get; set; }

        [Required]
        public int level_required { get; set; }
        [Required]
        public int points_required { get; set; }

        [Required]
        public int download_count { get; set; }

        public int status { get; set; }

        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public int report_count { get; set; }



        [ForeignKey("category_id")]
        public virtual Resource_categories Resource_Categories { get; set; }

        // 下载记录的导航属性
        public virtual ICollection<Download_log> Download_Logs { get; set; } = new List<Download_log>();
    }
}
