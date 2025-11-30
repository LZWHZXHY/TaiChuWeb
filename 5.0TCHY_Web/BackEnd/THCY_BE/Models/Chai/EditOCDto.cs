// Dto/Chai/EditOCDto.cs
using Microsoft.AspNetCore.Http;

namespace THCY_BE.Dto.Chai
{
    public class EditOCDto
    {
        public string? name { get; set; }
        public int? gender { get; set; }
        public int? age { get; set; }
        public string? species { get; set; }
        public string? ability { get; set; }
        public IFormFile? CharacterImage { get; set; }
        public string? character { get; set; }
        public string? background { get; set; }
        public string? colors { get; set; }
        public string? weaponImageUrl { get; set; }
        public string? weaponDesc { get; set; }
        public string? extraDesc { get; set; }
        public int? ocStatus { get; set; }
        public string? POO { get; set; }
        public int? currentTime { get; set; }
        public string? editDescription { get; set; }
    }
}