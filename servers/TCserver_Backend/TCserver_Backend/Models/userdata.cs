using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TCserver_Backend.Models.Resources;

namespace TCserver_Backend.Models
{
    [Table("userdata")]
    public class userdata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // 明确指定ID来自外部
        public int id { get; set; }
        [Required]
        public int points { get; set; } = 0;
        [Required]
        public int level { get; set; } = 0;
        [Required]
        public int exp { get; set; } = 0;
        
        public string title { get; set; }
        [Required]
        public DateTime lastlogin { get; set; } = DateTime.Now;

        public string logo { get; set; }
        public string background { get; set; }

        public int likes { get; set; }


        public DateTime? last_active_time { get; set; }


        public int byd { get; set; } = 0; // 是否参与BYD计划

        public int creater { get; set; } = 0; // 是否是创作者










        [ForeignKey("id")]
        public virtual useraccount useraccount { get; set; }

        public virtual ICollection<Download_logs> DownloadLogs { get; set; } = new List<Download_logs>();
    }
}
