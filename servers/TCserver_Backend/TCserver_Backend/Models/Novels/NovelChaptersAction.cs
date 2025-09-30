using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models.Novels
{
    [Table("novel_chapters_actions")]

    public class NovelChaptersAction
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        public int chapterId { get; set; }
        [Required]
        public int userId { get; set; }
        [Required]
        public int actionType { get; set; } // 1=like, 2=report,3=bookmark,4=unlike,5=unreport,6=remove bookmark,7=view

        [Required]
        public DateTime actionTime { get; set; }
    }
}
