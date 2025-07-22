using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để cập nhật thông tin project
    /// </summary>
    public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Tên project là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên project không được quá 100 ký tự")]
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