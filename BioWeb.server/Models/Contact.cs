using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = "";

        [StringLength(255)]
        public string Address { get; set; } = "";

        [StringLength(255)]
        public string GitHubURL { get; set; } = "";

        [StringLength(255)]
        public string LinkedInURL { get; set; } = "";

        [StringLength(255)]
        public string FacebookURL { get; set; } = "";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
