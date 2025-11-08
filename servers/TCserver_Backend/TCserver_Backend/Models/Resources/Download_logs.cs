using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Resources
{
    [Table("download_logs")]
    public class Download_logs
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int user_id { get; set; }
        [Required]
        public int resource_id { get; set; }

        [Required]
        public int points_cost { get; set; }

        [Required]
        public DateTime download_time { get; set; }

        [ForeignKey("user_id")]
        public virtual userdata userdata { get; set; }
        [ForeignKey("resource_id")]
        public virtual Resources Resources { get; set; }


    }
} 
