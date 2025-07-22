using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho thông tin project
    /// </summary>
    public class ProjectResponse
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

    /// <summary>
    /// Response của project
    /// </summary>
    public class ProjectApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}