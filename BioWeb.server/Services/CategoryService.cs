using BioWeb.Server.Data;
using BioWeb.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BioWeb.Server.Services
{
    /// <summary>
    /// Interface cho CategoryService - định nghĩa các method cần có
    /// </summary>
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
    }

    /// <summary>
    /// Service để quản lý category - làm việc với database
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor - inject database context vào đây
        /// </summary>
        /// <param name="context">Database context</param>
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả category - kèm theo số lượng bài viết luôn
        /// </summary>
        /// <returns>Danh sách category</returns>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Articles) // Load luôn articles để đếm
                .ToListAsync();
        }

        /// <summary>
        /// Lấy category theo ID - có thể null nếu không tìm thấy
        /// </summary>
        /// <param name="id">ID của category</param>
        /// <returns>Category hoặc null</returns>
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Articles)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
        }

        /// <summary>
        /// Tạo category mới - admin tạo thôi nhé
        /// </summary>
        /// <param name="category">Category cần tạo</param>
        /// <returns>Category đã tạo</returns>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Update category - sửa tên category chẳng hạn
        /// </summary>
        /// <param name="category">Category cần update</param>
        /// <returns>True nếu thành công</returns>
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            _context.Entry(category).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CategoryExists(category.CategoryID))
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
        /// Xóa category - cẩn thận nhé, xóa rồi các bài viết sẽ mất category
        /// </summary>
        /// <param name="id">ID category cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            // Kiểm tra xem có bài viết nào đang dùng category này không
            var hasArticles = await _context.Articles.AnyAsync(a => a.CategoryID == id);
            if (hasArticles)
            {
                // Không xóa được vì còn bài viết đang dùng
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Kiểm tra category có tồn tại không
        /// </summary>
        /// <param name="id">ID category</param>
        /// <returns>True nếu tồn tại</returns>
        private async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(e => e.CategoryID == id);
        }
    }
}
