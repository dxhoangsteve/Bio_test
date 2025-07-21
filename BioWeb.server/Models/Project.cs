using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }

        [Required]
        [StringLength(100)]
        public string ProjectName { get; set; } = null!;

        public string Description { get; set; } = "";

        [StringLength(255)]
        public string GitHubURL { get; set; } = "";

        [StringLength(255)]
        public string ProjectURL { get; set; } = "";

        [StringLength(255)]
        public string ThumbnailURL { get; set; } = "";

        [StringLength(255)]
        public string Technologies { get; set; } = "";
        
        public int DisplayOrder { get; set; } = 0;
        
        public bool IsPublished { get; set; } = true;
    }
}