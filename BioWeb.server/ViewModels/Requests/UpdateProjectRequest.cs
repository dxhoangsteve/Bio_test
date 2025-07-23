using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Custom validation attribute cho Technologies field
    /// </summary>
    public class TechnologiesValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return true; // Cho phép empty

            var technologies = value.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Kiểm tra số lượng technologies không quá 5
            if (technologies.Length > 5)
            {
                ErrorMessage = "Chỉ được phép tối đa 5 technologies";
                return false;
            }

            // Kiểm tra mỗi technology không quá 50 ký tự
            foreach (var tech in technologies)
            {
                if (tech.Trim().Length > 50)
                {
                    ErrorMessage = "Mỗi technology không được quá 50 ký tự";
                    return false;
                }
            }

            return true;
        }
    }

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

        [TechnologiesValidation]
        public string Technologies { get; set; } = "";

        public int DisplayOrder { get; set; } = 0;

        public bool IsPublished { get; set; } = true;
    }
}