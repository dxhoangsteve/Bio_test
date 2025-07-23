namespace BioWeb.client.Models
{
    /// <summary>
    /// DTO cho Project data từ API
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
    }

    /// <summary>
    /// Response từ API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// DTO cho About Me data
    /// </summary>
    public class AboutMeDto
    {
        public string FullName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = "";
    }

    /// <summary>
    /// DTO cho Contact data
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
    /// DTO cho Site Configuration data
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
    public class ArticleDto
    {
        public int ArticleID { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; } = "";
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
    }
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public ICollection <ArticleDto> Articles { get; set; } = new List<ArticleDto>();
    }
}
