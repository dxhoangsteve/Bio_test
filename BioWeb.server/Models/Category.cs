using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BioWeb.Server.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = null!;

        // Mối quan hệ: Một chuyên mục có nhiều bài viết
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}