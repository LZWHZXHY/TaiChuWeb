using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace THCY_BE.Dto.Drawing
{
    public class DrawingUploadDto
    {
        [Required]
        public IFormFile Image { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Desc { get; set; }

        // 可选：作者昵称，前端可不传则取 Claim 的 Name
        [MaxLength(100)]
        public string AuthorName { get; set; }
    }
}
