using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để cập nhật About Me
    /// </summary>
    public class UpdateAboutMeRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Chức danh là bắt buộc")]
        [StringLength(100, ErrorMessage = "Chức danh tối đa 100 ký tự")]
        public string JobTitle { get; set; } = null!;

        [StringLength(255, ErrorMessage = "URL avatar tối đa 255 ký tự")]
        public string AvatarURL { get; set; } = "";

        [Required(ErrorMessage = "Mô tả bản thân là bắt buộc")]
        public string BioSummary { get; set; } = null!;
    }
}
