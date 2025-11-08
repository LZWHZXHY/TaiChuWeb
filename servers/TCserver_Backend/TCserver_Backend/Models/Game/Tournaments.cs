using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace TCserver_Backend.Models.Game
{
    [Table("tournaments")]
    public class Tournaments
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public int status { get; set; } // 0: Scheduled, 1: Ongoing, 2: Completed, 3: Cancelled

        [Required]
        public int gameType { get; set; } // 0:CSGO, 1: DOTA2, 2: LOL, etc.

        [Required]
        public DateTime startDate { get; set; }
        
        public DateTime? endDate { get; set; }

        [Required]
        public string rules { get; set; }

        [Required]
        public string requirements { get; set; }

        [Required]
        public string reward { get; set; }

        public int? joinAmount { get; set; } // 可选，允许为null

        [Required]
        public DateTime registrationDate { get; set; } //报名截止日期

        public string? backgroundUrl { get; set; } // 可选，允许为null

        [Required]
        public string joinUrl { get; set; }

        [Required]
        public string summary { get; set; } 
    }
}
