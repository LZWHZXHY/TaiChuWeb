using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("blockusers")]
    public class BlockUsers
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // 拉黑者用户ID

        public int? BlockID { get; set; }  // 被拉黑的用户ID（允许为空）

        [Column("CreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("UserId")]
        public virtual userdata User { get; set; }

        [ForeignKey("BlockID")]
        public virtual userdata BlockedUser { get; set; }
    }
}