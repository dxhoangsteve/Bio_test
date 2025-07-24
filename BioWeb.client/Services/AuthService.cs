using Microsoft.JSInterop;

namespace BioWeb.client.Services
{
    public interface IAuthService
    {
        Task<bool> IsAdminAsync();
        Task<string?> GetTokenAsync();
        Task LogoutAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IApiService _apiService;

        public AuthService(IJSRuntime jsRuntime, IApiService apiService)
        {
            _jsRuntime = jsRuntime;
            _apiService = apiService;
        }

        /// <summary>
        /// Kiểm tra xem user có phải admin không
        /// </summary>
        public async Task<bool> IsAdminAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return false;
                }

                // Validate token với server
                return await _apiService.ValidateTokenAsync(token);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy JWT token từ localStorage
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "adminToken");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Logout admin
        /// </summary>
        public async Task LogoutAsync()
        {
            await _apiService.ClearAuthTokenAsync();
        }
    }
}
