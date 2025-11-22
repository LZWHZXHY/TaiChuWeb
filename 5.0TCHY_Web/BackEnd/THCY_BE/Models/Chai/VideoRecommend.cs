using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Chai
{
    [Table("video_recommend")]
    public class VideoRecommend
    {
        [Key]
        [Required]
        public int ID { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public string video_url { get; set; }
        [Required]
        public int category_id { get; set; } // 1 是接力类的视频，2是联合类的视频，3是世界观企划类的，

        [Required]
        public string author { get; set; }

        public int? Bestyear { get; set; }

        public int? monthRecommend { get; set; }

    }
}
