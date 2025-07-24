using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Models;
using BioWeb.Server.ViewModels.Requests;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Attributes;
using BioWeb.Server.Services;
using BioWeb.Shared.Models.DTOs;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        /// <summary>
        /// Lấy tất cả bài viết - admin xem được hết, kể cả chưa publish
        /// </summary>
        [HttpGet("admin")]
        [AdminAuth]
        public async Task<ActionResult<ArticleApiResponse<IEnumerable<ArticleResponse>>>> GetAllArticlesForAdmin()
        {
            try
            {
                var articles = await _articleService.GetAllArticlesAsync();
                var articleResponses = articles.Select(a => new ArticleResponse
                {
                    ArticleID = a.ArticleID,
                    Title = a.Title,
                    Content = a.Content,
                    ThumbnailURL = a.ThumbnailURL,
                    IsPublished = a.IsPublished,
                    CreatedAt = a.CreatedAt,
                    CategoryID = a.CategoryID,
                    CategoryName = a.Category.CategoryName // Lấy tên category
                });

                return Ok(new ArticleApiResponse<IEnumerable<ArticleResponse>>
                {
                    Success = true,
                    Message = "Lấy danh sách bài viết thành công",
                    Data = articleResponses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<IEnumerable<ArticleResponse>>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy tất cả bài viết - phân biệt admin và public
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ArticleApiResponse<IEnumerable<object>>>> GetArticles()
        {
            try
            {
                // Kiểm tra xem có phải admin không
                var isAdmin = IsAdminRequest();

                if (isAdmin)
                {
                    // Admin: Lấy tất cả bài viết với đầy đủ thông tin
                    var allArticles = await _articleService.GetAllArticlesAsync();
                    var adminResponses = allArticles.Select(a => new ArticleResponse
                    {
                        ArticleID = a.ArticleID,
                        Title = a.Title,
                        Content = a.Content,
                        ThumbnailURL = a.ThumbnailURL,
                        IsPublished = a.IsPublished,
                        CreatedAt = a.CreatedAt,
                        CategoryID = a.CategoryID,
                        CategoryName = a.Category?.CategoryName ?? ""
                    });

                    return Ok(new ArticleApiResponse<IEnumerable<object>>
                    {
                        Success = true,
                        Message = "Lấy tất cả bài viết thành công (Admin)",
                        Data = adminResponses
                    });
                }
                else
                {
                    // Public: Chỉ lấy bài viết đã publish với ít thông tin
                    var publishedArticles = await _articleService.GetPublishedArticlesAsync();
                    var publicResponses = publishedArticles.Select(a => new PublicArticleResponse
                    {
                        ArticleID = a.ArticleID,
                        Title = a.Title,
                        Content = a.Content,
                        CreatedAt = a.CreatedAt,
                        CategoryName = a.Category?.CategoryName ?? ""
                    });

                    return Ok(new ArticleApiResponse<IEnumerable<object>>
                    {
                        Success = true,
                        Message = "Lấy bài viết công khai thành công",
                        Data = publicResponses
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<IEnumerable<object>>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }



        /// <summary>
        /// Lấy bài viết theo ID - guest chỉ xem được bài đã publish
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleApiResponse<PublicArticleResponse>>> GetPublishedArticle(int id)
        {
            try
            {
                var article = await _articleService.GetPublishedArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new ArticleApiResponse<PublicArticleResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết này hoặc bài viết chưa được publish"
                    });
                }

                var response = new PublicArticleResponse
                {
                    ArticleID = article.ArticleID,
                    Title = article.Title,
                    Content = article.Content,
                    CreatedAt = article.CreatedAt,
                    CategoryName = article.Category.CategoryName
                };

                return Ok(new ArticleApiResponse<PublicArticleResponse>
                {
                    Success = true,
                    Message = "Lấy bài viết thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<PublicArticleResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy bài viết theo ID cho admin - xem được hết, kể cả chưa publish
        /// </summary>
        [HttpGet("admin/{id}")]
        [AdminAuth]
        public async Task<ActionResult<ArticleApiResponse<ArticleResponse>>> GetArticleForAdmin(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new ArticleApiResponse<ArticleResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết này"
                    });
                }

                var response = new ArticleResponse
                {
                    ArticleID = article.ArticleID,
                    Title = article.Title,
                    Content = article.Content,
                    ThumbnailURL = article.ThumbnailURL,
                    IsPublished = article.IsPublished,
                    CreatedAt = article.CreatedAt,
                    CategoryID = article.CategoryID,
                    CategoryName = article.Category.CategoryName
                };

                return Ok(new ArticleApiResponse<ArticleResponse>
                {
                    Success = true,
                    Message = "Lấy bài viết thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<ArticleResponse>
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Tạo bài viết mới - chỉ admin mới được tạo
        /// </summary>
        [HttpPost]
        [AdminAuth]
        public async Task<ActionResult<ArticleApiResponse<ArticleResponse>>> CreateArticle([FromBody] CreateArticleRequest request)
        {
            // Kiểm tra ModelState validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                return BadRequest(new ArticleApiResponse<ArticleResponse>
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ",
                    Errors = errors
                });
            }

            try
            {
                var article = new Article
                {
                    Title = request.Title,
                    Content = request.Content,
                    IsPublished = request.IsPublished,
                    CategoryID = request.CategoryID
                };

                var createdArticle = await _articleService.CreateArticleAsync(article);

                var response = new ArticleResponse
                {
                    ArticleID = createdArticle.ArticleID,
                    Title = createdArticle.Title,
                    Content = createdArticle.Content,
                    ThumbnailURL = createdArticle.ThumbnailURL,
                    IsPublished = createdArticle.IsPublished,
                    CreatedAt = createdArticle.CreatedAt,
                    CategoryID = createdArticle.CategoryID,
                    CategoryName = createdArticle.Category?.CategoryName ?? ""
                };

                return Created($"/api/Article/{createdArticle.ArticleID}", new ArticleApiResponse<ArticleResponse>
                {
                    Success = true,
                    Message = "Tạo bài viết thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<ArticleResponse>
                {
                    Success = false,
                    Message = $"Tạo bài viết thất bại: {ex.Message}"
                });
            }
        }



        /// <summary>
        /// Cập nhật bài viết - admin only
        /// </summary>
        [HttpPut("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ArticleApiResponse<ArticleResponse>>> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new ArticleApiResponse<ArticleResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                // Cập nhật thông tin
                article.Title = request.Title;
                article.Content = request.Content;
                article.IsPublished = request.IsPublished;
                article.CategoryID = request.CategoryID;

                var result = await _articleService.UpdateArticleAsync(article);

                if (!result)
                {
                    return BadRequest(new ArticleApiResponse<ArticleResponse>
                    {
                        Success = false,
                        Message = "Cập nhật bài viết thất bại"
                    });
                }

                // Lấy lại article sau khi update
                var updatedArticle = await _articleService.GetArticleByIdAsync(id);

                var response = new ArticleResponse
                {
                    ArticleID = updatedArticle.ArticleID,
                    Title = updatedArticle.Title,
                    Content = updatedArticle.Content,
                    ThumbnailURL = updatedArticle.ThumbnailURL,
                    IsPublished = updatedArticle.IsPublished,
                    CreatedAt = updatedArticle.CreatedAt,
                    CategoryID = updatedArticle.CategoryID,
                    CategoryName = updatedArticle.Category?.CategoryName ?? ""
                };

                return Ok(new ArticleApiResponse<ArticleResponse>
                {
                    Success = true,
                    Message = "Cập nhật bài viết thành công",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleApiResponse<ArticleResponse>
                {
                    Success = false,
                    Message = $"Cập nhật bài viết thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa bài viết - admin only
        /// </summary>
        [HttpDelete("{id}")]
        [AdminAuth]
        public async Task<ActionResult<ArticleSimpleResponse>> DeleteArticle(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new ArticleSimpleResponse
                    {
                        Success = false,
                        Message = "Không tìm thấy bài viết"
                    });
                }

                await _articleService.DeleteArticleAsync(id);

                return Ok(new ArticleSimpleResponse
                {
                    Success = true,
                    Message = "Xóa bài viết thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ArticleSimpleResponse
                {
                    Success = false,
                    Message = $"Xóa bài viết thất bại: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Kiểm tra xem request có phải từ admin không
        /// </summary>
        private bool IsAdminRequest()
        {
            // Kiểm tra JWT token
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return User.IsInRole("Admin");
            }

            // Kiểm tra admin headers (legacy)
            var username = Request.Headers["X-Admin-Username"].FirstOrDefault();
            var password = Request.Headers["X-Admin-Password"].FirstOrDefault();

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }
    }

    /// <summary>
    /// Response đơn giản cho Article
    /// </summary>
    public class ArticleSimpleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}
