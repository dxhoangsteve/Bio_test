namespace BioWeb.Server.ViewModels.DTOs
{
    /// <summary>
    /// DTO cho thông tin Contact
    /// </summary>
    public class ContactDto
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
}
