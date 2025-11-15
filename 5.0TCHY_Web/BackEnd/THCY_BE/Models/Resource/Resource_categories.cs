using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace THCY_BE.Models.Resource
{
    [Table("resource_categories")]
    public class Resource_categories
    {
        [Key]
        [Required]
        public int ID { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public int sort_order { get; set; }

        public int status { get; set; }

        public DateTime create_time { get; set; }

        public DateTime update_time { get; set; }

        public virtual ICollection<Resources> Resources { get; set; } = new List<Resources>();
    }
}
