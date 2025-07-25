using Microsoft.AspNetCore.Mvc;
using BioWeb.Server.Attributes;
using BioWeb.Server.ViewModels.Responses;
using BioWeb.Server.Services;

namespace BioWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;
        private readonly ISiteConfigurationService _siteConfigService;

        // Allowed file types
        private readonly string[] _allowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedDocumentTypes = { ".pdf", ".doc", ".docx" };

        // Max file sizes (in bytes)
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
        private const long MaxDocumentSize = 10 * 1024 * 1024; // 10MB

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger, ISiteConfigurationService siteConfigService)
        {
            _environment = environment;
            _logger = logger;
            _siteConfigService = siteConfigService;
        }

        /// <summary>
        /// Upload avatar cho admin (chỉ admin mới upload được)
        /// </summary>
        [HttpPost("avatar")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadAvatar(IFormFile file, [FromForm] string? oldFileName = null, [FromForm] bool autoSave = false)
        {
            try
            {
                _logger.LogInformation("UploadAvatar called with autoSave: {AutoSave}", autoSave);

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    DeleteOldFile("avatars", oldFileName);
                }

                var result = await ProcessImageUpload(file, "avatars");
                if (result.Success && autoSave)
                {
                    _logger.LogInformation("AutoSave is enabled, updating avatar URL in database");
                    // Tự động cập nhật URL vào database
                    await UpdateAvatarUrlInDatabase(result.Data!.Url);
                }
                else if (result.Success)
                {
                    _logger.LogInformation("Upload successful but autoSave is disabled");
                }

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
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadProjectThumbnail(IFormFile file, [FromForm] string? oldFileName = null)
        {
            try
            {
                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    DeleteOldFile("projects", oldFileName);
                }

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
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadArticleThumbnail(IFormFile file, [FromForm] string? oldFileName = null)
        {
            try
            {
                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    DeleteOldFile("articles", oldFileName);
                }

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
        /// Test endpoint để debug AutoSave
        /// </summary>
        [HttpPost("test-autosave")]
        [AdminAuth]
        public ActionResult<object> TestAutoSave([FromForm] bool autoSave = false, [FromForm] string testValue = "")
        {
            _logger.LogInformation("TestAutoSave called with autoSave: {AutoSave}, testValue: {TestValue}", autoSave, testValue);
            return Ok(new { autoSave = autoSave, testValue = testValue, message = "Test successful" });
        }

        /// <summary>
        /// Kiểm tra database hiện tại
        /// </summary>
        [HttpGet("check-database")]
        [AdminAuth]
        public async Task<ActionResult<object>> CheckDatabase()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();
                return Ok(new
                {
                    configId = config.ConfigID,
                    fullName = config.FullName,
                    avatarURL = config.AvatarURL,
                    updatedAt = config.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Test CV endpoint
        /// </summary>
        [HttpGet("cv/test")]
        [AdminAuth]
        public ActionResult<object> TestCVEndpoint()
        {
            _logger.LogInformation("CV test endpoint called");
            return Ok(new { message = "CV endpoint is working", timestamp = DateTime.Now });
        }

        /// <summary>
        /// Download CV file (public endpoint)
        /// </summary>
        [HttpGet("cv/download")]
        public async Task<IActionResult> DownloadCV()
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();

                if (string.IsNullOrEmpty(config.CV_FilePath))
                {
                    return NotFound(new { message = "CV file not found" });
                }

                // Extract filename from URL
                var fileName = Path.GetFileName(new Uri(config.CV_FilePath).LocalPath);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "cv", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "CV file not found on server" });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading CV");
                return BadRequest(new { message = "Error downloading CV" });
            }
        }

        /// <summary>
        /// Upload CV file (chỉ admin)
        /// </summary>
        [HttpPost("cv")]
        [AdminAuth]
        public async Task<ActionResult<ApiResponse<UploadResponse>>> UploadCV(IFormFile file, [FromForm] string? oldFileName = null, [FromForm] string? autoSave = null)
        {
            try
            {
                // Debug form data
                _logger.LogInformation("Form data received:");
                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("  {Key}: {Value}", key, Request.Form[key]);
                }

                // Parse autoSave parameter
                bool autoSaveEnabled = !string.IsNullOrEmpty(autoSave) &&
                                      (autoSave.ToLower() == "true" || autoSave == "1");

                _logger.LogInformation("UploadCV called with file: {FileName}, oldFileName: {OldFileName}, autoSave: {AutoSave} (parsed: {AutoSaveEnabled})",
                                     file?.FileName, oldFileName, autoSave, autoSaveEnabled);

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(oldFileName))
                {
                    DeleteOldFile("cv", oldFileName);
                }

                var result = await ProcessDocumentUpload(file, "cv");
                if (result.Success && autoSaveEnabled)
                {
                    _logger.LogInformation("AutoSave is enabled, updating CV URL in database");
                    // Tự động cập nhật URL vào database
                    await UpdateCVUrlInDatabase(result.Data!.Url);
                }
                else if (result.Success)
                {
                    _logger.LogInformation("Upload successful but autoSave is disabled");
                }

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
        /// Lấy thông tin file đã upload
        /// </summary>
        [HttpGet("file-info/{category}/{fileName}")]
        public ActionResult<ApiResponse<FileInfoResponse>> GetFileInfo(string category, string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", category);
                var filePath = Path.Combine(uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new ApiResponse<FileInfoResponse>
                    {
                        Success = false,
                        Message = "File không tồn tại"
                    });
                }

                var fileInfo = new FileInfo(filePath);
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                return Ok(new ApiResponse<FileInfoResponse>
                {
                    Success = true,
                    Message = "Lấy thông tin file thành công",
                    Data = new FileInfoResponse
                    {
                        FileName = fileName,
                        Url = $"{baseUrl}/uploads/{category}/{fileName}",
                        Size = fileInfo.Length,
                        CreatedAt = fileInfo.CreationTime,
                        LastModified = fileInfo.LastWriteTime
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info");
                return BadRequest(new ApiResponse<FileInfoResponse>
                {
                    Success = false,
                    Message = "Lỗi lấy thông tin file"
                });
            }
        }

        /// <summary>
        /// Xóa file đã upload (chỉ admin)
        /// </summary>
        [HttpDelete("file/{category}/{fileName}")]
        [AdminAuth]
        public ActionResult<ApiResponse<object>> DeleteFile(string category, string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", category);
                var filePath = Path.Combine(uploadsPath, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "File không tồn tại"
                    });
                }

                System.IO.File.Delete(filePath);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Xóa file thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return BadRequest(new ApiResponse<object>
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
            await SaveFile(file, category, fileName);

            // Get server base URL
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Upload thành công",
                Data = new UploadResponse
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = $"{baseUrl}/uploads/{category}/{fileName}",
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
            await SaveFile(file, category, fileName);

            // Get server base URL
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return new ApiResponse<UploadResponse>
            {
                Success = true,
                Message = "Upload thành công",
                Data = new UploadResponse
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    Url = $"{baseUrl}/uploads/{category}/{fileName}",
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

        private static string GenerateFileName(string originalFileName)
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

            _logger.LogInformation("File saved: {FilePath}", filePath);
            return filePath;
        }

        /// <summary>
        /// Xóa file cũ
        /// </summary>
        private void DeleteOldFile(string category, string fileName)
        {
            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", category);
                var filePath = Path.Combine(uploadsPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("Deleted old file: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old file: {FileName}", fileName);
                // Không throw exception vì việc xóa file cũ không quan trọng bằng upload file mới
            }
        }

        /// <summary>
        /// Tự động cập nhật avatar URL vào database
        /// </summary>
        private async Task UpdateAvatarUrlInDatabase(string avatarUrl)
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();
                config.AvatarURL = avatarUrl;
                await _siteConfigService.UpdateSiteConfigurationAsync(config);
                _logger.LogInformation("Updated avatar URL in database: {AvatarUrl}", avatarUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update avatar URL in database: {AvatarUrl}", avatarUrl);
                // Không throw exception vì upload đã thành công
            }
        }

        /// <summary>
        /// Tự động cập nhật CV URL vào database
        /// </summary>
        private async Task UpdateCVUrlInDatabase(string cvUrl)
        {
            try
            {
                var config = await _siteConfigService.GetOrCreateDefaultConfigAsync();
                config.CV_FilePath = cvUrl;
                await _siteConfigService.UpdateSiteConfigurationAsync(config);
                _logger.LogInformation("Updated CV URL in database: {CVUrl}", cvUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update CV URL in database: {CVUrl}", cvUrl);
                // Không throw exception vì upload đã thành công
            }
        }

        /// <summary>
        /// Get content type based on file extension
        /// </summary>
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }

}
