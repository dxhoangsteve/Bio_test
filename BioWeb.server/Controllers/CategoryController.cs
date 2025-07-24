using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Models;
using BioWeb.Server.ViewModels.Requests;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Attributes;
using BioWeb.Server.Services;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Lấy tất cả category - ai cũng xem được, guest và admin đều ok
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CategoryApiResponse<IEnumerable<CategoryResponse>>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                var categoryResponses = categories.Select(c => new CategoryResponse
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    ArticleCount = c.Articles.Count // Đếm số bài viết trong category
                });

                return Ok(new CategoryApiResponse<IEnumerable<CategoryResponse>>
                {
                    Success = true,
                    Message = "Lấy danh sách category thành công rồi",
                    Data = categoryResponses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new CategoryApiResponse<IEnumerable<CategoryResponse>>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy category theo ID - ai cũng xem được
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryApiResponse<CategoryResponse>>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new CategoryApiResponse<CategoryResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy category này"
                    });
                }

                var response = new CategoryResponse
                {
                    CategoryID = category.CategoryID,
                    CategoryName = category.CategoryName,
                    ArticleCount = category.Articles.Count
                };

                return Ok(new CategoryApiResponse<CategoryResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin category thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new CategoryApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo category mới - chỉ admin mới được tạo nhé
        /// </summary>
        [HttpPost]
        [AdminAuth]
        public async Task<ActionResult<CategoryApiResponse<CategoryResponse>>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            // Kiểm tra ModelState validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new CategoryApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = errors
                });
            }

            try
            {
                var category = new Category
                {
                    CategoryName = request.CategoryName
                };

                var createdCategory = await _categoryService.CreateCategoryAsync(category);

                var response = new CategoryResponse
                {
                    CategoryID = createdCategory.CategoryID,
                    CategoryName = createdCategory.CategoryName,
                    ArticleCount = 0 // Mới tạo thì chưa có bài viết nào
                };

                return Created($"/api/Category/{createdCategory.CategoryID}", new CategoryApiResponse<CategoryResponse>
                {
                    Success = true,
                    Message = "Tạo category thành công rồi",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new CategoryApiResponse<CategoryResponse>
                {
                    Success = false,
                    Message = $"Tạo category thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Cập nhật category - chỉ admin mới được sửa
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<CategorySimpleResponse>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new CategorySimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy category để sửa"
                    });
                }

                // Cập nhật thông tin
                category.CategoryName = request.CategoryName;

                var result = await _categoryService.UpdateCategoryAsync(category);
                if (result)
                {
                    return Ok(new CategorySimpleResponse
                    {
                        Success = true,
                        Message = "Cập nhật category thành công"
                    });
                }
                else
                {
                    return BadRequest(new CategorySimpleResponse
                    {
                        Success = false,
                        Message = "Cập nhật category thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new CategorySimpleResponse
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa category - chỉ admin mới được xóa, và phải cẩn thận
        /// </summary>
        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<CategorySimpleResponse>> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (result)
                {
                    return Ok(new CategorySimpleResponse
                    {
                        Success = true,
                        Message = "Xóa category thành công"
                    });
                }
                else
                {
                    return BadRequest(new CategorySimpleResponse
                    {
                        Success = false,
                        Message = "Không thể xóa category này, có thể vì còn bài viết đang dùng"
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new CategorySimpleResponse
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }






    }

    /// <summary>
    /// Response đơn giản cho Category
    /// </summary>
    public class CategorySimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
