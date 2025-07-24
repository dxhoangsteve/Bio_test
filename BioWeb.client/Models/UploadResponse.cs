namespace BioWeb.client.Models
{
    public class UploadResponse
    {
        public string FileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Url { get; set; } = "";
        public long Size { get; set; }
        public string ContentType { get; set; } = "";
    }
}
