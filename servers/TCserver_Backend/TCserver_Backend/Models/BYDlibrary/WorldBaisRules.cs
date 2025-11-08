using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.BYDlibrary
{
    [Table("worldbasicrules")]
    public class WorldBaisRules
    {
        

        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public DateTime UpdateTime { get; set; }
    }
}
