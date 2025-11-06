using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TCserver_Backend.Models
{
    [Table("refresh_tokens")]
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(128)]
        public string Token { get; set; }

        [Required]
        public DateTime Expiry { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [ForeignKey("UserId")]
        public virtual useraccount User { get; set; }
    }
}