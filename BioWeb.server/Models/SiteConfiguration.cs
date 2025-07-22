using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class SiteConfiguration
    {
        [Key]
        public int ConfigID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(100)]
        public string JobTitle { get; set; } = "";

        [StringLength(255)]
        public string AvatarURL { get; set; } = "";

        public string BioSummary { get; set; } = "";

        [StringLength(100)]
        public string Email { get; set; } = "";

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

        [StringLength(255)]
        public string CV_FilePath { get; set; } = "";

        public int ViewCount { get; set; } = 0;

        public DateTime UpdatedAt { get; set; }
    }
}