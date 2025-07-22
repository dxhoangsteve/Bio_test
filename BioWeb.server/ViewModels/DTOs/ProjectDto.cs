namespace BioWeb.Server.ViewModels.DTOs
{
    /// <summary>
    /// DTO cho thông tin project
    /// </summary>
    public class ProjectDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; } = null!;
        public string Description { get; set; } = "";
        public string GitHubURL { get; set; } = "";
        public string ProjectURL { get; set; } = "";
        public string ThumbnailURL { get; set; } = "";
        public string Technologies { get; set; } = "";
        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
    }
}