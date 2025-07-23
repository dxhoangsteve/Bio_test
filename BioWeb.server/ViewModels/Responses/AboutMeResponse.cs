namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho About Me - public
    /// </summary>
    public class AboutMeResponse
    {
        public string FullName { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = null!;
    }

    /// <summary>
    /// Response model cho About Me - admin (có thêm ID và UpdatedAt)
    /// </summary>
    public class AboutMeAdminResponse
    {
        public int AboutMeID { get; set; }
        public string FullName { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response model cho thông tin Contact - public
    /// </summary>
    public class ContactResponse
    {
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public string FacebookURL { get; set; } = "";
    }

    /// <summary>
    /// Response model cho Contact - admin (có thêm ID và UpdatedAt)
    /// </summary>
    public class ContactAdminResponse
    {
        public int ContactID { get; set; }
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public string FacebookURL { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response model chung cho Site API
    /// </summary>
    public class SiteApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Response đơn giản cho Site
    /// </summary>
    public class SiteSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
