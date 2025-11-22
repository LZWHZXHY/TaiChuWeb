using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace THCY_BE.Dto.Chai
{
    public class OCBattleMultipleDto
    {
        [Required]
        [StringLength(50)]
        public string OCName { get; set; } = string.Empty;

        [Required]
        public int gender { get; set; } // 0=男 1=女 2=未知

        [Required]
        public int age { get; set; }

        [Required]
        public string species { get; set; } = string.Empty;

        [Required]
        public string ability { get; set; } = string.Empty;

        [Required]
        public string authorName { get; set; } = string.Empty;

        [Required]
        public int currentTime { get; set; }

        public string? Background { get; set; }

        [Required(ErrorMessage = "POO是必须的")]
        [StringLength(100, ErrorMessage = "POO不能超过100个字符")]
        public string POO { get; set; } = string.Empty;

        [Required(ErrorMessage = "角色立绘是必须的")]
        public IFormFile CharacterImage { get; set; } = null!;

        public IFormFile? BattleSceneImage { get; set; }
    }

    public class OCBattleUpdateDto
    {
        [StringLength(50)]
        public string? OCName { get; set; }

        public int? gender { get; set; }

        public int? age { get; set; }

        public string? species { get; set; }

        public string? ability { get; set; }

        public string? Background { get; set; }

        [StringLength(100, ErrorMessage = "POO不能超过100个字符")]
        public string? POO { get; set; }

        public int? currentTime { get; set; }

        public IFormFile? CharacterImage { get; set; }
    }
}