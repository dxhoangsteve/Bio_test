namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho upload file
    /// </summary>
    public class UploadResponse
    {
        public string FileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Url { get; set; } = "";
        public long Size { get; set; }
        public string ContentType { get; set; } = "";
    }

    /// <summary>
    /// Response model cho thông tin file
    /// </summary>
    public class FileInfoResponse
    {
        public string FileName { get; set; } = "";
        public string Url { get; set; } = "";
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
    }
}
