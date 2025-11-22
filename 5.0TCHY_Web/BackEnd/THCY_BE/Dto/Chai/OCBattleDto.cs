using System.ComponentModel.DataAnnotations;

namespace THCY_BE.Dto.Chai
{
    public class OCBattleDto
    {
        [Required]
        [StringLength (50)]
        public string OCName { get; set; }
        [Required]
        public int gender { get; set; } //OC 性别 0=男 1=女 2=未知
        [Required]
        public int age { get; set; } //OC 年龄
        [Required]
        public string species { get; set; } //OC 种族
        [Required]
        public string ability { get; set; } //OC 能力
        [Required]
        public string authorName { get; set; } //OC 作者名字
        [Required]
        public int currentTime { get; set; } //OC 当前时间
        public string? Background { get; set; } //OC 背景故事
        

        [Required(ErrorMessage = "POO是必须的")] // 强制要求
        [StringLength(100, ErrorMessage = "POO不能超过100个字符")]
        public string POO { get; set; } = string.Empty; // 移除默认值null

        [Required(ErrorMessage = "角色立绘是必须的")]
        public IFormFile CharacterImage { get; set; } = null!;
    }
}
