using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để update bài viết - admin muốn sửa gì thì sửa
    /// </summary>
    public class UpdateArticleRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề quá dài rồi, 200 ký tự thôi")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung bài viết phải có chứ")]
        public string Content { get; set; } = null!;

        public bool IsPublished { get; set; }

        [Required(ErrorMessage = "Category phải chọn")]
        public int CategoryID { get; set; }
    }
}
