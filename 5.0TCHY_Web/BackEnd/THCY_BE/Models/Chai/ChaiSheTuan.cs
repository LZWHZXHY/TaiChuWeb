using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Chai
{
    [Table("shetuan")]
    public class ChaiSheTuan
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string leader { get; set; }

        [Required]
        public int type { get; set; }

        [Required]
        public int test { get; set; } //表示这个社团是否需要考核

        [Required]
        public int testlevel { get; set; } //考核难度等级，如果需要考核的话

        [Required]
        public string url { get; set; } //加群链接

        [Required]
        public int size { get; set; } //社团大小

        [Required]
        public string desc { get; set; }

        [Required]
        public int verify { get; set; } //是否通过审核
    }
}
