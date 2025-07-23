namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho thông tin About Me - Bio summary
    /// </summary>
    public class AboutMeResponse
    {
        public string FullName { get; set; } = null!;
        public string JobTitle { get; set; } = "";
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = "";
    }

    /// <summary>
    /// Response model cho thông tin Contact
    /// </summary>
    public class ContactInfoResponse
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string LinkedInURL { get; set; } = "";
        public string FacebookURL { get; set; } = "";
    }
}
