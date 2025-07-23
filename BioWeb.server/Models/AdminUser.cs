using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class AdminUser
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        public DateTime? LastLogin { get; set; }
    }
}