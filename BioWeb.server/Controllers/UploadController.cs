using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Attributes;
using BioWeb.Server.ViewModels.Responses;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;

        // Allowed file types
        private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedDocumentTypes = { ".pdf", ".doc", ".docx" };
        
        // Max file sizes (in bytes)
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
        private const long MaxDocumentSize = 10 * 1024 * 1024; // 10MB

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Upload avatar cho admin (chỉ admin mới upload được)
        /// </summary>
        [HttpPost("avatar")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadAvatar(IFormFile file)
        {
            try
            {
                var result = await ProcessImageUpload(file, "avatars");
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Lỗi upload avatar"
                });
            }
        }

        /// <summary>
        /// Upload thumbnail cho project (chỉ admin)
        /// </summary>
        [HttpPost("project-thumbnail")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadProjectThumbnail(IFormFile file)
        {
            try
            {
                var result = await ProcessImageUpload(file, "projects");
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading project thumbnail");
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Lỗi upload project thumbnail"
                });
            }
        }

        /// <summary>
        /// Upload thumbnail cho article (chỉ admin)
        /// </summary>
        [HttpPost("article-thumbnail")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadArticleThumbnail(IFormFile file)
        {
            try
            {
                var result = await ProcessImageUpload(file, "articles");
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading article thumbnail");
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Lỗi upload article thumbnail"
                });
            }
        }

        /// <summary>
        /// Upload CV file (chỉ admin)
        /// </summary>
        [HttpPost("cv")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadCV(IFormFile file)
        {
            try
            {
                var result = await ProcessDocumentUpload(file, "cv");
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV");
                return BadRequest(new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Lỗi upload CV"
                });
            }
        }

        /// <summary>
        /// Xóa file (chỉ admin)
        /// </summary>
        [HttpDelete("{category}/{fileName}")]
        [AdminAuth]
        public ActionResult<SimpleResponse> DeleteFile(string category, string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", category);
                var filePath = Path.Combine(uploadsPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"Deleted file: {filePath}");
                    
                    return Ok(new SimpleResponse
                    {
                        Success = true,
                        Message = "Xóa file thành công"
                    });
                }

                return NotFound(new SimpleResponse
                {
                    Success = false,
                    Message = "File không tồn tại"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {category}/{fileName}");
                return BadRequest(new SimpleResponse
                {
                    Success = false,
                    Message = "Lỗi xóa file"
                });
            }
        }

        #region Private Methods

        private async Task<ApiResponse<UploadResponse>> ProcessImageUpload(IFormFile file, string category)
        {
            // Validate file
            var validation = ValidateImageFile(file);
            if (!validation.Success)
            {
                return validation;
            }

            // Generate unique filename
            var fileName = GenerateFileName(file.FileName);
            
            // Save file
            var filePath = await SaveFile(file, category, fileName);
            
            return new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Upload thành công",
                Data = new UploadResponse
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = $"/uploads/{category}/{fileName}",
                    Size = file.Length,
                    ContentType = file.ContentType
                }
            };
        }

        private async Task<ApiResponse<UploadResponse>> ProcessDocumentUpload(IFormFile file, string category)
        {
            // Validate file
            var validation = ValidateDocumentFile(file);
            if (!validation.Success)
            {
                return validation;
            }

            // Generate unique filename
            var fileName = GenerateFileName(file.FileName);
            
            // Save file
            var filePath = await SaveFile(file, category, fileName);
            
            return new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Upload thành công",
                Data = new UploadResponse
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = $"/uploads/{category}/{fileName}",
                    Size = file.Length,
                    ContentType = file.ContentType
                }
            };
        }

        private ApiResponse<UploadResponse> ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Không có file nào được chọn"
                };
            }

            if (file.Length > MaxImageSize)
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"File quá lớn. Tối đa {MaxImageSize / (1024 * 1024)}MB"
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageTypes.Contains(extension))
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", _allowedImageTypes)}"
                };
            }

            return new ApiResponse<UploadResponse> { Success = true };
        }

        private ApiResponse<UploadResponse> ValidateDocumentFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = "Không có file nào được chọn"
                };
            }

            if (file.Length > MaxDocumentSize)
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"File quá lớn. Tối đa {MaxDocumentSize / (1024 * 1024)}MB"
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedDocumentTypes.Contains(extension))
            {
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"Định dạng file không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", _allowedDocumentTypes)}"
                };
            }

            return new ApiResponse<UploadResponse> { Success = true };
        }

        private string GenerateFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            return fileName;
        }

        private async Task<string> SaveFile(IFormFile file, string category, string fileName)
        {
            // Ensure directory exists
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", category);
            Directory.CreateDirectory(uploadsPath);

            // Save file
            var filePath = Path.Combine(uploadsPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"File saved: {filePath}");
            return filePath;
        }

        #endregion
    }

    /// <summary>
    /// Response model cho upload
    /// </summary>
    public class UploadResponse
    {
        public string FileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Url { get; set; } = "";
        public long Size { get; set; }
        public string ContentType { get; set; } = "";
    }
}
