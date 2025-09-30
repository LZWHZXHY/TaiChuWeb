using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Game
{
    [Table("champions")]
    public class Champions
    {
        [Key]
        [Required]

        public int id { get; set; }

        [Required]
        public int tournamentId { get; set; } //关联的比赛ID
        [Required]
        public string name { get; set; }

        [Required]
        public int championID { get; set; } //用于存放参赛人员在官网的账户的id 也就是在userdate中的userID
    }
}
