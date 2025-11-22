using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace THCY_BE.Models.Chai
{
    [Table("chailianhe")]
    public class ChaiLianHe
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string host { get; set; }

        [Required]
        public int type { get; set; }

        [Required]
        public DateTime startdate { get; set; }

        public DateTime? enddate { get; set; }

        public string desc { get; set; } = string.Empty;

        [Column("QQgroup")]
        [JsonPropertyName("QQgroup")] // 显式指定 JSON 属性名
        public string QQgroup { get; set; } = string.Empty;

        public string require { get; set; } = string.Empty;

        [Required]
        public int verify { get; set; }
        [Required]
        public int report { get; set; }
    }
}
