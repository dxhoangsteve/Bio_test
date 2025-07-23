namespace BioWeb.Server.ViewModels.DTOs
{
    /// <summary>
    /// DTO cho thông tin cấu hình gửi cho user hoặc quest
    /// </summary>
    public class SiteConfigurationDto
    {
        public int ConfigID { get; set; }
        public string FullName { get; set; } = null!;
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
