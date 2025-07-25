using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.ViewModels.Requests
{
    /// <summary>
    /// Request model để tạo bài viết mới - admin viết bài thôi nhé
    /// </summary>
    public class CreateArticleRequest
    {
        [Required(ErrorMessage = "Tiêu đề bài viết bắt buộc phải có")]
        [StringLength(200, ErrorMessage = "Tiêu đề dài quá rồi, tối đa 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Nội dung bài viết không thể để trống")]
        public string Content { get; set; } = null!;

        public string ThumbnailURL { get; set; } = "";

        public bool IsPublished { get; set; } = false; // Mặc định chưa publish

        [Required(ErrorMessage = "Phải chọn category cho bài viết")]
        public int CategoryID { get; set; }
    }
}
