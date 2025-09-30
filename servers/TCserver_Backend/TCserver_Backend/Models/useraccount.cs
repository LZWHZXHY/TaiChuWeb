using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;




//这里是用户数据模型，一行用户数据的结构。一般是第一步
namespace TCserver_Backend.Models
{
    [Table("useraccount")]
    public class useraccount
    {
        public int Id { get; set; }

        [Required]

        public string username { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varbinary(128)")]
        public byte[] password_hash { get; set; }

        [Required]
        [Column("email_address")]
        public string email_address { get; set; } = string.Empty;

        public DateTime date { get; set; } = DateTime.Now; //注册时间
        public int state { get; set; } //0表示离线，1表示在线，2表示封禁
        public int rank { get; set; } //0表示普通用户，1表示制作者账户，2表示管理员

        public bool is_verified { get; set; } = false;

        public int failed_login_attempts { get; set; } = 0;
        public DateTime? last_failed_login { get; set; }

        //public DateTime? verified_at { get; set; }

        public virtual userdata userdata { get; set; }
        public virtual UserCheckinRecord usercheckinrecord { get; set; }

        public virtual ICollection<Posts> Posts { get; set; } = new List<Posts>();

        public virtual ICollection<Comments> Comments { get; set; } = new List<Comments>();
    }
}