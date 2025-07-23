using BioWeb.client.Models;
using System.Text.Json;
using System.Net;


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

        // Blog methods
        Task<List<ArticleDto>> GetPublishedArticlesAsync();
        Task<List<ArticleDto>> GetArticlesByCategoryAsync(int categoryId);
        Task<ArticleDto?> GetArticleByIdAsync(int id);
        Task<List<CategoryDto>> GetCategoriesAsync();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public event Action<bool, string>? ServerStatusChanged;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
        /// Lấy thông tin About Me
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
        /// Lấy bài viết theo category
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

    }
}
