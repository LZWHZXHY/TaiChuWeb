using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Trade
{
    [Table("shopitem")]
    public class ShopItem
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string desc { get; set; }

        [Required]
        public string icon { get; set; } // 图标URL
        [Required]
        public int cost { get; set; } // 单价，单位分
        [Required]
        public int category { get; set; } // 类别，预留字段
        [Required]
        public int stock { get; set; } // 库存，-1表示无限
        [Required]
        public int enable { get; set; } // 是否上架 0:下架 1:上架

        [Required]
        public int level { get; set; } // 道具等级

        public int? requireLevel { get; set; } // 购买所需用户等级
    }
}
