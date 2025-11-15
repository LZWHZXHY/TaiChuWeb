using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.UserDate
{
    [Table("usercheckinrecord")]
    public class UserCheckIn
    {
        [Key]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Column("checkindate")]
        public DateTime CheckinDate { get; set; }

        [Column("type")]
        public int Type { get; set; } //签到类型，0为每日签到，1为补签签到, 2为活动签到

        [Required]
        [Column("user_id")]
        public int UserId { get; set; } //用户ID


        [ForeignKey("UserId")]
        public virtual UserAccount useraccount { get; set; }

    }
}
