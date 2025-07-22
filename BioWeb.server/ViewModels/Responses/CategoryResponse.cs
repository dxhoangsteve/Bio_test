namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho category - cái này ai cũng xem được
    /// </summary>
    public class CategoryResponse
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ArticleCount { get; set; } // Đếm xem có bao nhiêu bài viết trong category này
    }

    /// <summary>
    /// Response model chung cho Category API - kiểu wrapper ấy
    /// </summary>
    public class CategoryApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Response đơn giản 
    /// </summary>
    public class CategorySimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
