namespace BioWeb.client.Models
{
    /// <summary>
    /// Response model cho thông tin Contact - public (client-side)
    /// </summary>
    public class ContactResponse
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public string FacebookURL { get; set; } = "";
        public string CV_FilePath { get; set; } = "";
    }

    /// <summary>
    /// Response model cho thông tin site đầy đủ (admin) - client-side
    /// </summary>
    public class SiteConfigurationResponse
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

    /// <summary>
    /// Response của site configuration API (client-side)
    /// </summary>
    public class SiteConfigurationApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
