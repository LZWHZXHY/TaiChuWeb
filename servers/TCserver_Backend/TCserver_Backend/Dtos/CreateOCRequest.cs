using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Dtos
{
    public class CreateOCRequest
    {
        [Required, StringLength(200)]
        public string Name { get; set; } = "";

        [Required]
        
        public int Age { get; set; }

        [Required]
        [Range(0, 2)]
        public int Gender { get; set; } = 2; // 0=男,1=女,2=未知

        [Required, StringLength(100)]
        public string Species { get; set; } = "";

        [Required, StringLength(200)]
        public string POO { get; set; } = "";

        // 修改为 int 与模型保持一致（避免强转问题）
        [Required]
        public int OC_Current_Time { get; set; } = 0;

        [Required]
        [StringLength(4000)]
        public string ability { get; set; } = ""; // 前端传 JSON 字符串或纯文本

        [Required]
        [StringLength(500)]
        public string Colors { get; set; } = ""; // 配色 JSON 或文本

        // 必填：experience（模型里为 Required）
        [Required]
        public int[] Experience { get; set; } = Array.Empty<int>();

        // 可选字段
        [StringLength(4000)]
        public string? Background { get; set; }

        [StringLength(1000)]
        public string? OC_WeapenDesc { get; set; }

        public string? AuthorName { get; set; }

        [StringLength(2000)]
        public string? Character { get; set; } // 新增：人物性格/描述
    }
}