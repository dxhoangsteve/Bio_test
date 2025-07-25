using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho ArticleService - định nghĩa các method cần có
    /// </summary>
    public interface IArticleService
    {
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<IEnumerable<Article>> GetPublishedArticlesAsync();
        Task<IEnumerable<Article>> GetPublishedArticlesByCategoryAsync(int categoryId);

        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetPublishedArticleByIdAsync(int id);
        Task<Article> CreateArticleAsync(Article article);
        Task<bool> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
    }

    /// <summary>
    /// Service để quản lý bài viết - làm việc với database
    /// </summary>
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor - inject database context vào đây
        /// </summary>
        /// <param name="context">Database context</param>
        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả bài viết - admin xem được hết, kể cả chưa publish
        /// </summary>
        /// <returns>Danh sách tất cả bài viết</returns>
        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Category) // Load thông tin category
                .OrderByDescending(a => a.CreatedAt) // Mới nhất lên đầu
                .ToListAsync();
        }

        /// <summary>
        /// Lấy bài viết đã publish - guest chỉ xem được những bài này thôi
        /// </summary>
        /// <returns>Danh sách bài viết đã publish</returns>
        public async Task<IEnumerable<Article>> GetPublishedArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Category)
                .Where(a => a.IsPublished) // Chỉ lấy bài đã publish
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy bài viết đã publish theo category - guest lọc bài viết theo chủ đề
        /// </summary>
        /// <param name="categoryId">ID của category</param>
        /// <returns>Danh sách bài viết đã publish trong category</returns>
        public async Task<IEnumerable<Article>> GetPublishedArticlesByCategoryAsync(int categoryId)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .Where(a => a.IsPublished && a.CategoryID == categoryId) // Chỉ lấy bài đã publish và đúng category
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }



        /// <summary>
        /// Lấy bài viết theo ID - admin xem được hết
        /// </summary>
        /// <param name="id">ID bài viết</param>
        /// <returns>Bài viết hoặc null</returns>
        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.ArticleID == id);
        }

        /// <summary>
        /// Lấy bài viết đã publish theo ID - guest chỉ xem được bài đã publish
        /// </summary>
        /// <param name="id">ID bài viết</param>
        /// <returns>Bài viết đã publish hoặc null</returns>
        public async Task<Article?> GetPublishedArticleByIdAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.ArticleID == id && a.IsPublished);
        }

        /// <summary>
        /// Tạo bài viết mới - admin tạo thôi nhé
        /// </summary>
        /// <param name="article">Bài viết cần tạo</param>
        /// <returns>Bài viết đã tạo</returns>
        public async Task<Article> CreateArticleAsync(Article article)
        {
            article.CreatedAt = DateTime.UtcNow; // Set thời gian tạo
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Load lại để có đầy đủ thông tin Author và Category
            return await GetArticleByIdAsync(article.ArticleID) ?? article;
        }

        /// <summary>
        /// Update bài viết - admin sửa gì thì sửa
        /// </summary>
        /// <param name="article">Bài viết cần update</param>
        /// <returns>True nếu thành công</returns>
        public async Task<bool> UpdateArticleAsync(Article article)
        {
            _context.Entry(article).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ArticleExists(article.ArticleID))
                {
                    return false;
                }
                else
                {
                    throw; // Có lỗi gì khác thì throw lên
                }
            }
        }

        /// <summary>
        /// Xóa bài viết - admin muốn xóa thì xóa
        /// </summary>
        /// <param name="id">ID bài viết cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return false;
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra bài viết có tồn tại không
        /// </summary>
        /// <param name="id">ID bài viết</param>
        /// <returns>True nếu tồn tại</returns>
        private async Task<bool> ArticleExists(int id)
        {
            return await _context.Articles.AnyAsync(e => e.ArticleID == id);
        }
    }
}
