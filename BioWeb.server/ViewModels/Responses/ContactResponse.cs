namespace BioWeb.Server.ViewModels.Responses
{
    /// <summary>
    /// Response model cho admin 
    /// </summary>
    public class ContactResponse
    {
        public int ContactID { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public int ReadCount { get; set; }
    }

    /// <summary>
    /// Response model cho admin 
    /// </summary>
    public class ContactResponseForQuest
    {
        public string Email { get; set; } = null!;
    }

    public class ContactApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    public class ContactSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}