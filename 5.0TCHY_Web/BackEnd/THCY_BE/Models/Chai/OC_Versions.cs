using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Chai
{
    [Table("oc_versions")]
    public class OC_Versions
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public int ocMasterId { get; set; } // 关联主表ID


        [Required]
        public int versionNumber { get; set; } // 版本号（从1开始）


        [Required]
        public string versionDescription { get; set; } = "初始版本"; // 版本描述

        [Required]
        public bool isCurrent { get; set; } = false; // 是否为当前版本

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
        
        public string? OC_image_url { get; set; } //立绘 URL

        public string? character { get; set; } //OC 性格

        public string? background { get; set; } //OC 背景故事

        public string? colors { get; set; } //OC 颜色方案



        public string? OC_WeapenImgUrl { get; set; } //OC 武器图片 URL

        public string? OC_WeapenDesc { get; set; } //OC 武器描述

        public string? ExtraDesc { get; set; } //OC 额外描述

        // 添加这两个重要字段
        [Required]
        public DateTime createTime { get; set; } // 该版本的创建时间

        public int? OC_status { get; set; }

        [Required]
        public int worldTime { get; set; }


        [Required]
        public string experience { get; set; } = "[]"; //OC 经历 - 由后端管理，默认空数组

        public int winCount { get; set; }

        public int loseCount { get; set; }

        [Required]
        public string POO { get; set; }

        [ForeignKey("ocMasterId")]
        public virtual OC_Master OCMaster { get; set; }


    }
}
