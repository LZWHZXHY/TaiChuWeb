using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Chai
{
    [Table("oc_master")]
    public class OC_Master
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        public string name { get; set; } // OC 名字（主名称）

        [Required]
        public string authorName { get; set; } // OC 作者名字

        [Required]
        public int authorID { get; set; } // OC 作者 ID

        [Required]
        public DateTime createTime { get; set; } // OC 创建时间

        [Required]
        public DateTime updateTime { get; set; } // 最后更新时间

        [Required]
        public int currentVersionId { get; set; } // 当前最新版本的ID

        [Required]
        public int status { get; set; } // 主记录状态

        [Required]
        public int dueling { get; set; }





        // 导航属性
        public virtual ICollection<OC_Versions> Versions { get; set; }
    }
}
