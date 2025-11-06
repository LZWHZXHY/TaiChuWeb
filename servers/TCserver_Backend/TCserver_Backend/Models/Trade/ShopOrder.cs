using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Trade
{
    [Table("shoporder")]
    public class ShopOrder
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int userId { get; set; }
        [Required]
        public int itemId { get; set; }
        [Required]
        public int count { get; set; }
        [Required]
        public int cost { get; set; } // 总价，单位分
        [Required]
        public DateTime createTime { get; set; }
        [Required]
        public int status { get; set; } // 0:未支付 1:已支付 2:已取消


    }
}
