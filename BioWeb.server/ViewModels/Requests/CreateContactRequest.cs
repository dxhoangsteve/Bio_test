using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để tạo/cập nhật Contact
    /// </summary>
    public class CreateContactRequest
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; } = null!;

        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string PhoneNumber { get; set; } = "";

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string Address { get; set; } = "";

        [StringLength(255, ErrorMessage = "GitHub URL tối đa 255 ký tự")]
        public string GitHubURL { get; set; } = "";

        [StringLength(255, ErrorMessage = "LinkedIn URL tối đa 255 ký tự")]
        public string LinkedInURL { get; set; } = "";

        [StringLength(255, ErrorMessage = "Facebook URL tối đa 255 ký tự")]
        public string FacebookURL { get; set; } = "";
    }
}
