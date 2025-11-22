using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Chai
{
    [Table("oc_info")]
    public class OC_Info
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string name { get; set; } //OC 名字

        [Required]
        public int gender { get; set; } //OC 性别 0=男 1=女 2=未知

        [Required]
        public int age { get; set; } //OC 年龄
        [Required]
        public string species { get; set; } //OC 种族
        [Required]
        public string ability { get; set; } //OC 能力
        
        public string? OC_image_url { get; set; } //OC 头像 URL

        public string? character { get; set; } //OC 性格

        public string? background { get; set; } //OC 背景故事

        public string? colors { get; set; } //OC 颜色方案
        [Required]
        public string authorName { get; set; } //OC 作者名字
        [Required]
        public int authorID { get; set; } //OC 作者 ID

        public string? OC_WeapenImgUrl { get; set; } //OC 武器图片 URL

        public string? OC_WeapenDesc { get; set; } //OC 武器描述

        public string? ExtraDesc { get; set; } //OC 额外描述
        [Required]
        public DateTime createTime { get; set; } //OC 创建时间`
        [Required]
        public DateTime updateTime { get; set; } //OC 更新时间
        [Required]
        public int status { get; set; }

        [Required]
        public int dueling { get; set; }

        public int? OC_status { get; set; }

        [Required]
        public int OC_Current_Time { get; set; }

        [Required]
        public int version { get; set; }

        [Required]
        public string experience { get; set; } = "[]"; //OC 经历 - 由后端管理，默认空数组

        public int winCount { get; set; }

        public int loseCount { get; set; }

        [Required]
        public string POO { get; set; }




    }
}
