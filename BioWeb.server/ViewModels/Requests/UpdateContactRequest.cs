using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để cập nhật thông tin contact
    /// </summary>
    public class UpdateContactRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Tin nhắn là bắt buộc")]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; }
    }
}