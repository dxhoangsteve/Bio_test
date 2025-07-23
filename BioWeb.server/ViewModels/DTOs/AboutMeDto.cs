namespace BioWeb.Server.ViewModels.DTOs
{
    /// <summary>
    /// DTO cho thông tin About Me
    /// </summary>
    public class AboutMeDto
    {
        public int AboutMeID { get; set; }
        public string FullName { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public string AvatarURL { get; set; } = "";
        public string BioSummary { get; set; } = null!;
        public DateTime UpdatedAt { get; set; }
    }
}
