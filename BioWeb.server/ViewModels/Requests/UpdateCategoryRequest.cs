using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để update category - admin muốn sửa gì thì sửa
    /// </summary>
    public class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Tên category không được để trống nhé")]
        [StringLength(100, ErrorMessage = "Tên category dài quá rồi, 100 ký tự thôi")]
        public string CategoryName { get; set; } = null!;
    }
}
