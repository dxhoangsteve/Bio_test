namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Base response class cho tất cả API responses
    /// </summary>
    public class SimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Generic API response class
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
