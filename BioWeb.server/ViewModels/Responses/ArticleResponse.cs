namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho bài viết đầy đủ - admin xem được hết
    /// </summary>
    public class ArticleResponse
    {
        public int ArticleID { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ThumbnailURL { get; set; } = "";
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; } = ""; // Tên tác giả cho dễ đọc
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = ""; // Tên category cho dễ hiểu
    }

    /// <summary>
    /// Response model cho guest - chỉ xem bài đã publish thôi
    /// </summary>
    public class PublicArticleResponse
    {
        public int ArticleID { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string ThumbnailURL { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = "";
        public string CategoryName { get; set; } = "";
    }

    /// <summary>
    /// Response model ngắn gọn cho danh sách bài viết
    /// </summary>
    public class ArticleSummaryResponse
    {
        public int ArticleID { get; set; }
        public string Title { get; set; } = null!;
        public string ThumbnailURL { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string AuthorName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public bool IsPublished { get; set; }
    }

    /// <summary>
    /// Response model chung cho Article API
    /// </summary>
    public class ArticleApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Response đơn giản cho Article
    /// </summary>
    public class ArticleSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
