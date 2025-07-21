using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để cập nhật thông tin site
    /// </summary>
    public class UpdateSiteConfigurationRequest
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự")]
        public string FullName { get; set; } = null!;
        
        [StringLength(100, ErrorMessage = "Job title không được quá 100 ký tự")]
        public string JobTitle { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "Avatar URL không được quá 255 ký tự")]
        [Url(ErrorMessage = "Avatar URL không hợp lệ")]
        public string AvatarURL { get; set; } = "";
        
        [StringLength(2000, ErrorMessage = "Bio summary không được quá 2000 ký tự")]
        public string BioSummary { get; set; } = "";
        
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string Address { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "CV file path không được quá 255 ký tự")]
        public string CV_FilePath { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "Facebook URL không được quá 255 ký tự")]
        [Url(ErrorMessage = "Facebook URL không hợp lệ")]
        public string FacebookURL { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "GitHub URL không được quá 255 ký tự")]
        [Url(ErrorMessage = "GitHub URL không hợp lệ")]
        public string GitHubURL { get; set; } = "";
        
        [StringLength(255, ErrorMessage = "LinkedIn URL không được quá 255 ký tự")]
        [Url(ErrorMessage = "LinkedIn URL không hợp lệ")]
        public string LinkedInURL { get; set; } = "";
    }
}
