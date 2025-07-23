using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class AboutMe
    {
        [Key]
        public int AboutMeID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string JobTitle { get; set; } = null!;

        [StringLength(255)]
        public string AvatarURL { get; set; } = "";

        [Required]
        public string BioSummary { get; set; } = null!;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
