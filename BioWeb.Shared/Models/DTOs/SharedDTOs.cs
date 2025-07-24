namespace BioWeb.Shared.Models.DTOs
{
    /// <summary>
    /// DTO cho Project
    /// </summary>
    public class ProjectDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = "";
        public string Description { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string ProjectURL { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public string Technologies { get; set; } = "";
        public int DisplayOrder { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ViewCount { get; set; }
    }

    /// <summary>
    /// DTO cho Article
    /// </summary>
    public class ArticleDto
    {
        public int ArticleID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
    }

    /// <summary>
    /// DTO cho Category
    /// </summary>
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public ICollection<ArticleDto> Articles { get; set; } = new List<ArticleDto>();
    }

    /// <summary>
    /// DTO cho About Me
    /// </summary>
    public class AboutMeDto
    {
        public int AboutMeID { get; set; }
        public string FullName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }



    /// <summary>
    /// DTO cho Contact Info (legacy từ SiteConfiguration)
    /// </summary>
    public class ContactInfoDto
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public string FacebookURL { get; set; } = "";
    }

    /// <summary>
    /// DTO cho SiteConfiguration
    /// </summary>
    public class SiteConfigurationDto
    {
        public int ConfigID { get; set; }
        public string FullName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string CV_FilePath { get; set; } = "";
        public string FacebookURL { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public int ViewCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
