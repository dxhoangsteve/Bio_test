using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BioWeb.Server.Models
{
    public class Article
    {
        [Key]
        public int ArticleID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [StringLength(500)]
        public string? ThumbnailURL { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Khóa ngoại tới Category
        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; } = null!;
    }
}