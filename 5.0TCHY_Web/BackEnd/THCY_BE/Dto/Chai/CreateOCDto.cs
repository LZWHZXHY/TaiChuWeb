// Dto/Chai/CreateOCDto.cs
using Microsoft.AspNetCore.Http;

namespace THCY_BE.Dto.Chai
{
    public class CreateOCDto
    {
        public string? OCName { get; set; }
        public string? authorName { get; set; }
        public int? gender { get; set; }
        public int? age { get; set; }
        public string? species { get; set; }
        public string? ability { get; set; }
        public IFormFile? CharacterImage { get; set; }
        // 可选，多张武器图片
        public IFormFile[]? WeaponImages { get; set; }
        public string? weaponDesc { get; set; }
        // 新增 extraDesc，避免 controller 使用时编译错误
        public string? extraDesc { get; set; }
        public string? character { get; set; }
        public string? background { get; set; }
        public string? colors { get; set; }
        public int? ocStatus { get; set; }
        public string? POO { get; set; }
        public int? currentTime { get; set; }
        public string? VersionDescription { get; set; }
    }

    public class UpdateOCDto
    {
        public string? name { get; set; }
        public int? gender { get; set; }
        public int? age { get; set; }
        public string? species { get; set; }
        public string? ability { get; set; }
        public IFormFile? CharacterImage { get; set; }
        // 可选，多张武器图片（编辑时可以追加）
        public IFormFile[]? WeaponImages { get; set; }
        public string? character { get; set; }
        public string? background { get; set; }
        public string? colors { get; set; }
        public string? weaponImageUrl { get; set; } // 保持兼容（字符串）
        public string? weaponDesc { get; set; }
        public string? extraDesc { get; set; }
        public int? ocStatus { get; set; }
        public string? POO { get; set; }
        public int? currentTime { get; set; }
        public string? updateDescription { get; set; }
    }
}