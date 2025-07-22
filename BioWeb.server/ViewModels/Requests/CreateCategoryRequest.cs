using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để tạo category mới - cái này admin dùng thôi nhé
    /// </summary>
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Tên category là bắt buộc rồi bạn ơi")]
        [StringLength(100, ErrorMessage = "Tên category dài quá, tối đa 100 ký tự thôi")]
        public string CategoryName { get; set; } = null!;
    }
}
