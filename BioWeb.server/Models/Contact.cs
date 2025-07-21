using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}