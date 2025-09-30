using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("financialtable")]
    public class Financial
    {
        [Key]
        [Required]
        public int index { get; set; }

        [Required]
        public string ZhiChu { get; set; }

        [Required]
        public string ZhiChuXiangMu { get; set; }

        [Required]
        public DateTime date { get; set; }

        [Required]
        public string ShouKuan { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public int PayReceive { get; set; } // 1 支出 0 收入

    }
}
