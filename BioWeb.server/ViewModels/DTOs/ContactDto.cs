namespace BioWeb.Server.ViewModels.DTOs
{
    /// <summary>
    /// DTO cho thông tin cấu hình gửi cho user hoặc quest
    /// </summary>
    public class ContactDto
    {
        public int ContactID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public int IsRead { get; set; }
        public DateTime SentDate { get; set; }
    }
}
