namespace BioWeb.Shared.Models.Responses
{
    /// <summary>
    /// Response model chung cho tất cả API calls
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Response đơn giản không có data
    /// </summary>
    public class SimpleApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public List<string> Errors { get; set; } = new();
    }
}
