// Dtos/CreatePostDto.cs
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Dtos
{
    public class CreatePostDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public int PostType { get; set; } // 0: 柴圈帖子, 1: 游戏帖子, 2: xx帖子

        public List<string>? Images { get; set; } // 图片路径
        public List<string>? Videos { get; set; } // 视频路径

        [Display(Name = "路径分隔符")]
        public string PathSeparator { get; set; } = ";";
    }
}