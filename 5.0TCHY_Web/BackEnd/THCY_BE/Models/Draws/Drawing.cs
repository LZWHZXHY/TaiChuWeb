using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace THCY_BE.Models.Draws
{
    [Table("drawingpicture")]
    public class Drawing
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int UploaderId { get; set; }

        [Required]
        public string AuthorName { get; set; }

        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public DateTime UploadAt { get; set; }

        [Required]
        public int status { get; set; }
       
        public int? likes { get; set; }
        public int? report { get; set; }

        [Required]
        public string title { get; set; }

        public string? desc { get; set; }


    }
}
