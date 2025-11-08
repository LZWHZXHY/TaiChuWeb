using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Game
{
    [Table("tournamentsignups")]
    public class TournamentSignups
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int tournamentId { get; set; } //关联的比赛ID
        [Required]
        public int userId { get; set; } //用户ID
        [Required]
        public DateTime signupTime { get; set; } //报名时间
    }
}
