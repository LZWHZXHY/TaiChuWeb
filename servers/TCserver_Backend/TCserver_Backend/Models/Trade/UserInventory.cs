using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Trade
{
    [Table("userinventory")]
    public class UserInventory
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
        public DateTime acquireTime { get; set; }
        [Required]
        public int status { get; set; } // 0:正常 1:已使用 2:已过期


        [ForeignKey("userId")]
        public virtual userdata userdata { get; set; }

        [ForeignKey("itemId")]
        public virtual ShopItem shopItem { get; set; }

    }




}
