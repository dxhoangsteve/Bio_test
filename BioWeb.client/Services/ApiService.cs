using BioWeb.Shared.Models.DTOs;
using BioWeb.Shared.Models.Responses;
using BioWeb.client.Models;
using System.Text.Json;
using System.Net;
using Microsoft.JSInterop;
using System.Text;


// api sử lý ở client 
namespace BioWeb.client.Services
{
    /// <summary>
    /// Service để gọi API từ server
    /// </summary>
    public interface IApiService
    {
        Task<List<ProjectDto>> GetPublishedProjectsAsync();
        Task<AboutMeDto?> GetAboutMeAsync();
        Task<bool> CheckServerHealthAsync();
        event Action<bool, string> ServerStatusChanged;
        Task<ContactInfoDto> GetContactInfoDto();

        Task<List<ArticleDto>> GetPublishedArticlesAsync();
        Task<List<ArticleDto>> GetArticlesByCategoryAsync(int categoryId);

        Task<ArticleDto?> GetArticleByIdAsync(int id);
        Task<List<CategoryDto>> GetCategoriesAsync();

        Task<LoginResponse> LoginAsync(string username, string password);

        // Admin Authentication methods
        Task SetAuthTokenAsync(string token);
        Task LoadAuthTokenAsync();
        Task ClearAuthTokenAsync();
        Task<bool> ValidateTokenAsync(string token);

        // Additional methods for dialogs
        Task<ContactInfoDto> GetContactAsync();

        // File Upload methods
        Task<ApiResponse<UploadResponse>> UploadFileAsync(string endpoint, MultipartFormDataContent content);
        Task<ApiResponse<FileInfoResponse>> GetFileInfoAsync(string category, string fileName);
        Task<SimpleResponse> DeleteFileAsync(string category, string fileName);

        // Admin CRUD methods
        Task<ApiResponse<AboutMeDto>> UpdateAboutMeAsync(AboutMeDto aboutMe);
        Task<ApiResponse<ContactInfoDto>> UpdateContactAsync(ContactInfoDto contact);

        // Project methods
        Task<ApiResponse<List<ProjectDto>>> GetProjectsAsync();
        Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id);
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(ProjectDto project);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, ProjectDto project);
        Task<SimpleResponse> DeleteProjectAsync(int id);

        // Category methods
        Task<ApiResponse<List<CategoryDto>>> GetCategoriesForAdminAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryForAdminAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto category);
        Task<SimpleResponse> UpdateCategoryAsync(int id, CategoryDto category);
        Task<SimpleResponse> DeleteCategoryAsync(int id);

        // Article methods
        Task<ApiResponse<List<ArticleDto>>> GetArticlesForAdminAsync();
        Task<ApiResponse<ArticleDto>> GetArticleForAdminAsync(int id);
        Task<ApiResponse<ArticleDto>> CreateArticleAsync(ArticleDto article);
        Task<ApiResponse<ArticleDto>> UpdateArticleAsync(int id, ArticleDto article);
        Task<SimpleResponse> DeleteArticleAsync(int id);

        // Site Configuration methods
        Task<ApiResponse<SiteConfigurationDto>> GetSiteConfigurationAsync();
        Task<SimpleResponse> UpdateSiteConfigurationAsync(SiteConfigurationDto siteConfig);
        Task<SimpleResponse> IncrementViewCountAsync();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IJSRuntime _jsRuntime;

        public event Action<bool, string>? ServerStatusChanged;

        public ApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Kiểm tra server có hoạt động không
        /// </summary>
        public async Task<bool> CheckServerHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Seed/check-data");
                var isHealthy = response.IsSuccessStatusCode;

                ServerStatusChanged?.Invoke(isHealthy, isHealthy ? "" : $"Server trả về: {response.StatusCode}");
                return isHealthy;
            }
            catch (HttpRequestException ex)
            {
                var errorMsg = "Không thể kết nối đến server";
                ServerStatusChanged?.Invoke(false, errorMsg);
                Console.WriteLine($"Server connection error: {ex.Message}");
                return false;
            }
            catch (TaskCanceledException)
            {
                var errorMsg = "Kết nối server bị timeout";
                ServerStatusChanged?.Invoke(false, errorMsg);
                Console.WriteLine("Server request timeout");
                return false;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Lỗi không xác định: {ex.Message}";
                ServerStatusChanged?.Invoke(false, errorMsg);
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách projects đã publish
        /// </summary>
        public async Task<List<ProjectDto>> GetPublishedProjectsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Project");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProjectDto>>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data ?? [];
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return [];
            }
            catch (HttpRequestException ex)
            {
                ServerStatusChanged?.Invoke(false, "Không thể kết nối đến server");
                Console.WriteLine($"Error fetching projects: {ex.Message}");
                return [];
            }
            catch (TaskCanceledException)
            {
                ServerStatusChanged?.Invoke(false, "Kết nối server bị timeout");
                Console.WriteLine("Projects request timeout");
                return [];
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching projects: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Lấy thông tin About Me từ SiteConfiguration
        /// </summary>
        public async Task<AboutMeDto?> GetAboutMeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/SiteConfiguration/about-me");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AboutMeDto>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data;
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return null;
            }
            catch (HttpRequestException ex)
            {
                ServerStatusChanged?.Invoke(false, "Không thể kết nối đến server");
                Console.WriteLine($"Error fetching about me: {ex.Message}");
                return null;
            }
            catch (TaskCanceledException)
            {
                ServerStatusChanged?.Invoke(false, "Kết nối server bị timeout");
                Console.WriteLine("About me request timeout");
                return null;
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching about me: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy thông tin contact của site và tăng view count
        /// </summary>
        public async Task<ContactInfoDto> GetContactInfoDto()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/SiteConfiguration/contact");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ContactInfoDto>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data ?? new ContactInfoDto();
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return new ContactInfoDto();
            }
            catch (HttpRequestException ex)
            {
                ServerStatusChanged?.Invoke(false, "Không thể kết nối đến server");
                Console.WriteLine($"Error fetching contact info: {ex.Message}");
                return new ContactInfoDto();
            }
            catch (TaskCanceledException)
            {
                ServerStatusChanged?.Invoke(false, "Kết nối server bị timeout");
                Console.WriteLine("Contact info request timeout");
                return new ContactInfoDto();
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching contact info: {ex.Message}");
                return new ContactInfoDto();
            }
        }

        /// <summary>
        /// Lấy danh sách bài viết đã publish
        /// </summary>
        public async Task<List<ArticleDto>> GetPublishedArticlesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Article");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ArticleDto>>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data ?? [];
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return [];
            }
            catch (HttpRequestException ex)
            {
                ServerStatusChanged?.Invoke(false, "Không thể kết nối đến server");
                Console.WriteLine($"Error fetching articles: {ex.Message}");
                return [];
            }
            catch (TaskCanceledException)
            {
                ServerStatusChanged?.Invoke(false, "Kết nối server bị timeout");
                Console.WriteLine("Articles request timeout");
                return [];
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching articles: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Lấy danh sách bài viết đã publish theo category
        /// </summary>
        public async Task<List<ArticleDto>> GetArticlesByCategoryAsync(int categoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Article/category/{categoryId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ArticleDto>>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data ?? [];
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return [];
            }
            catch (HttpRequestException ex)
            {
                ServerStatusChanged?.Invoke(false, "Không thể kết nối đến server");
                Console.WriteLine($"Error fetching articles by category: {ex.Message}");
                return [];
            }
            catch (TaskCanceledException)
            {
                ServerStatusChanged?.Invoke(false, "Kết nối server bị timeout");
                Console.WriteLine("Articles by category request timeout");
                return [];
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching articles by category: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// Lấy chi tiết bài viết theo ID
        /// </summary>
        public async Task<ArticleDto?> GetArticleByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Article/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data;
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching article: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy danh sách categories
        /// </summary>
        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Category");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse?.Data ?? [];
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return [];
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return [];
            }
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { username, password }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Auth/admin/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);

                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse;
                }

                ServerStatusChanged?.Invoke(false, $"API trả về: {response.StatusCode}");
                return new LoginResponse { Success = false, Message = "Đăng nhập thất bại" };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi không xác định: {ex.Message}");
                Console.WriteLine($"Error logging in: {ex.Message}");
                return new LoginResponse { Success = false, Message = "Đăng nhập thất bại" };
            }
        }

        #region Admin Authentication Methods

        /// <summary>
        /// Set Authorization header với JWT token
        /// </summary>
        public async Task SetAuthTokenAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Lưu token vào localStorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "adminToken", token);
        }

        /// <summary>
        /// Load token từ localStorage và set header
        /// </summary>
        public async Task LoadAuthTokenAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "adminToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        /// <summary>
        /// Clear auth token
        /// </summary>
        public async Task ClearAuthTokenAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "adminToken");
        }

        /// <summary>
        /// Validate JWT token với server
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Tạm thời set token để test
                var originalAuth = _httpClient.DefaultRequestHeaders.Authorization;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("/api/Auth/validate-token");

                // Restore original auth header
                _httpClient.DefaultRequestHeaders.Authorization = originalAuth;

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Additional Dialog Methods

        /// <summary>
        /// Get contact info for dialogs
        /// </summary>
        public async Task<ContactInfoDto> GetContactAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/SiteConfiguration/contact");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ContactInfoDto>(json, _jsonOptions);

                return apiResponse ?? new ContactInfoDto();
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Lỗi API GetContact: {ex.Message}");
                return new ContactInfoDto();
            }
        }

        #endregion

        #region File Upload Methods

        /// <summary>
        /// Upload file to server
        /// </summary>
        public async Task<ApiResponse<UploadResponse>> UploadFileAsync(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<UploadResponse>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<UploadResponse> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Upload failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<UploadResponse>>(json, _jsonOptions);
                return errorResponse ?? new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"Upload failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Upload error: {ex.Message}");
                return new ApiResponse<UploadResponse>
                {
                    Success = false,
                    Message = $"Upload error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get file information
        /// </summary>
        public async Task<ApiResponse<FileInfoResponse>> GetFileInfoAsync(string category, string fileName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Upload/file-info/{category}/{fileName}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<FileInfoResponse>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<FileInfoResponse> { Success = false, Message = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<FileInfoResponse>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(false, errorResponse?.Message ?? "Unknown error");
                    return errorResponse ?? new ApiResponse<FileInfoResponse>
                    {
                        Success = false,
                        Message = $"Get file info failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Get file info error: {ex.Message}");
                return new ApiResponse<FileInfoResponse>
                {
                    Success = false,
                    Message = $"Get file info error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete file
        /// </summary>
        public async Task<SimpleResponse> DeleteFileAsync(string category, string fileName)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Upload/file/{category}/{fileName}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(false, errorResponse?.Message ?? "Unknown error");
                    return errorResponse ?? new SimpleResponse
                    {
                        Success = false,
                        Message = $"Delete file failed: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Delete file error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete file error: {ex.Message}"
                };
            }
        }

        #endregion

        #region Admin CRUD Methods

        /// <summary>
        /// Update About Me information
        /// </summary>
        public async Task<ApiResponse<AboutMeDto>> UpdateAboutMeAsync(AboutMeDto aboutMe)
        {
            try
            {
                var request = new
                {
                    FullName = aboutMe.FullName,
                    JobTitle = aboutMe.JobTitle,
                    BioSummary = aboutMe.BioSummary
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/SiteConfiguration/about-me", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<AboutMeDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<AboutMeDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<AboutMeDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<AboutMeDto>
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new ApiResponse<AboutMeDto>
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update Contact information
        /// </summary>
        public async Task<ApiResponse<ContactInfoDto>> UpdateContactAsync(ContactInfoDto contact)
        {
            try
            {
                var request = new
                {
                    PhoneNumber = contact.PhoneNumber,
                    Address = contact.Address,
                    FacebookURL = contact.FacebookURL,
                    GitHubURL = contact.GitHubURL,
                    LinkedInURL = contact.LinkedInURL
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/SiteConfiguration/contact", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ContactInfoDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ContactInfoDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<ContactInfoDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<ContactInfoDto>
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new ApiResponse<ContactInfoDto>
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        #endregion

        #region Project Methods

        /// <summary>
        /// Get all projects for admin
        /// </summary>
        public async Task<ApiResponse<List<ProjectDto>>> GetProjectsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Project/admin");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProjectDto>>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<List<ProjectDto>> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<List<ProjectDto>>
                {
                    Success = false,
                    Message = $"Failed to get projects: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetProjects: {ex.Message}");
                return new ApiResponse<List<ProjectDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get project by ID for admin
        /// </summary>
        public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Project/admin/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ProjectDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Failed to get project: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetProject: {ex.Message}");
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new project
        /// </summary>
        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(ProjectDto project)
        {
            try
            {
                var request = new
                {
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    GitHubURL = project.GitHubURL,
                    ProjectURL = project.ProjectURL,
                    ThumbnailURL = project.ThumbnailURL,
                    Technologies = project.Technologies,
                    DisplayOrder = project.DisplayOrder,
                    IsPublished = project.IsPublished
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Project", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ProjectDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Create failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Create failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Create error: {ex.Message}");
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Create error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update project
        /// </summary>
        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(int id, ProjectDto project)
        {
            try
            {
                var request = new
                {
                    ProjectName = project.ProjectName,
                    Description = project.Description,
                    GitHubURL = project.GitHubURL,
                    ProjectURL = project.ProjectURL,
                    ThumbnailURL = project.ThumbnailURL,
                    Technologies = project.Technologies,
                    DisplayOrder = project.DisplayOrder,
                    IsPublished = project.IsPublished
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/Project/{id}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ProjectDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<ProjectDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new ApiResponse<ProjectDto>
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete project
        /// </summary>
        public async Task<SimpleResponse> DeleteProjectAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Project/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Delete failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Delete error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete error: {ex.Message}"
                };
            }
        }

        #endregion

        #region Category Methods

        /// <summary>
        /// Get all categories for admin
        /// </summary>
        public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesForAdminAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Category");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<List<CategoryDto>> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = $"Failed to get categories: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetCategories: {ex.Message}");
                return new ApiResponse<List<CategoryDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get category by ID for admin
        /// </summary>
        public async Task<ApiResponse<CategoryDto>> GetCategoryForAdminAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Category/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<CategoryDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Failed to get category: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetCategory: {ex.Message}");
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new category
        /// </summary>
        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CategoryDto category)
        {
            try
            {
                var request = new
                {
                    CategoryName = category.CategoryName,
                    Description = category.Description
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Category", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<CategoryDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Create failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Create failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Create error: {ex.Message}");
                return new ApiResponse<CategoryDto>
                {
                    Success = false,
                    Message = $"Create error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update category
        /// </summary>
        public async Task<SimpleResponse> UpdateCategoryAsync(int id, CategoryDto category)
        {
            try
            {
                var request = new
                {
                    CategoryName = category.CategoryName,
                    Description = category.Description
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/Category/{id}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(responseJson, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete category
        /// </summary>
        public async Task<SimpleResponse> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Category/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Delete failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Delete error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete error: {ex.Message}"
                };
            }
        }

        #endregion

        #region Article Methods

        /// <summary>
        /// Get all articles for admin
        /// </summary>
        public async Task<ApiResponse<List<ArticleDto>>> GetArticlesForAdminAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Article/admin");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ArticleDto>>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<List<ArticleDto>> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<List<ArticleDto>>
                {
                    Success = false,
                    Message = $"Failed to get articles: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetArticles: {ex.Message}");
                return new ApiResponse<List<ArticleDto>>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Get article by ID for admin
        /// </summary>
        public async Task<ApiResponse<ArticleDto>> GetArticleForAdminAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Article/admin/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ArticleDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Failed to get article: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetArticle: {ex.Message}");
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Create new article
        /// </summary>
        public async Task<ApiResponse<ArticleDto>> CreateArticleAsync(ArticleDto article)
        {
            try
            {
                var request = new
                {
                    Title = article.Title,
                    Content = article.Content,
                    ThumbnailURL = article.ThumbnailURL,
                    IsPublished = article.IsPublished,
                    CategoryID = article.CategoryID
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/Article", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ArticleDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Create failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Create failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Create error: {ex.Message}");
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Create error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update article
        /// </summary>
        public async Task<ApiResponse<ArticleDto>> UpdateArticleAsync(int id, ArticleDto article)
        {
            try
            {
                var request = new
                {
                    Title = article.Title,
                    Content = article.Content,
                    ThumbnailURL = article.ThumbnailURL,
                    IsPublished = article.IsPublished,
                    CategoryID = article.CategoryID
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/Article/{id}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<ArticleDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<ArticleDto>>(responseJson, _jsonOptions);
                return errorResponse ?? new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new ApiResponse<ArticleDto>
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete article
        /// </summary>
        public async Task<SimpleResponse> DeleteArticleAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/Article/{id}");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Delete failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Delete error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Delete error: {ex.Message}"
                };
            }
        }

        #endregion

        #region Site Configuration Methods

        /// <summary>
        /// Get site configuration for admin
        /// </summary>
        public async Task<ApiResponse<SiteConfigurationDto>> GetSiteConfigurationAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/SiteConfiguration");
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<SiteConfigurationDto>>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new ApiResponse<SiteConfigurationDto> { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"API returned: {response.StatusCode}");
                return new ApiResponse<SiteConfigurationDto>
                {
                    Success = false,
                    Message = $"Failed to get site configuration: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"API Error GetSiteConfiguration: {ex.Message}");
                return new ApiResponse<SiteConfigurationDto>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Update site configuration
        /// </summary>
        public async Task<SimpleResponse> UpdateSiteConfigurationAsync(SiteConfigurationDto siteConfig)
        {
            try
            {
                var request = new
                {
                    FullName = siteConfig.FullName,
                    JobTitle = siteConfig.JobTitle,
                    AvatarURL = siteConfig.AvatarURL,
                    BioSummary = siteConfig.BioSummary,
                    PhoneNumber = siteConfig.PhoneNumber,
                    Address = siteConfig.Address,
                    CV_FilePath = siteConfig.CV_FilePath,
                    FacebookURL = siteConfig.FacebookURL,
                    GitHubURL = siteConfig.GitHubURL,
                    LinkedInURL = siteConfig.LinkedInURL
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/SiteConfiguration/{siteConfig.ConfigID}", content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(responseJson, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Update failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(responseJson, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Update failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Update error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Update error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Tăng view count
        /// </summary>
        public async Task<SimpleResponse> IncrementViewCountAsync()
        {
            try
            {
                var response = await _httpClient.PutAsync("/api/SiteConfiguration/contact/view-count", null);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                    ServerStatusChanged?.Invoke(true, "");
                    return apiResponse ?? new SimpleResponse { Success = false, Message = "Invalid response" };
                }

                ServerStatusChanged?.Invoke(false, $"Increment view count failed: {response.StatusCode}");
                var errorResponse = JsonSerializer.Deserialize<SimpleResponse>(json, _jsonOptions);
                return errorResponse ?? new SimpleResponse
                {
                    Success = false,
                    Message = $"Increment view count failed: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                ServerStatusChanged?.Invoke(false, $"Increment view count error: {ex.Message}");
                return new SimpleResponse
                {
                    Success = false,
                    Message = $"Increment view count error: {ex.Message}"
                };
            }
        }

        #endregion
    }
}
