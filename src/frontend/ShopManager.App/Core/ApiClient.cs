using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ShopManager.Core.DTOs;
using ShopManager.Core.Settings;

namespace ShopManager.App.Core
{
    public class ApiClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string? _authToken;
        private static string _baseUrl = "http://localhost:5000";

        static ApiClient()
        {
            var settings = SettingsManager.Load();
            if (!string.IsNullOrWhiteSpace(settings.ApiUrl))
            {
                SetBaseUrl(settings.ApiUrl);
            }
            if (!string.IsNullOrWhiteSpace(settings.ApiToken))
            {
                SetAuthToken(settings.ApiToken);
            }
        }

        public static void SetBaseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            _baseUrl = url.TrimEnd('/');
        }

        public static void SetAuthToken(string token)
        {
            _authToken = token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public static async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var settings = SettingsManager.Load();
                SetBaseUrl(settings.ApiUrl);

                string requestUrl = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    string errContent = await response.Content.ReadAsStringAsync();
                    return ApiResponse<T>.Fail($"Lỗi API ({response.StatusCode}): {errContent}");
                }

                var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                return apiResult ?? ApiResponse<T>.Fail("Phản hồi API rỗng.");
            }
            catch (Exception ex)
            {
                return ApiResponse<T>.Fail($"Lỗi kết nối API: {ex.Message}");
            }
        }

        public static async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
        {
            try
            {
                var settings = SettingsManager.Load();
                SetBaseUrl(settings.ApiUrl);

                string requestUrl = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
                var response = await _httpClient.PostAsJsonAsync(requestUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    string errContent = await response.Content.ReadAsStringAsync();
                    return ApiResponse<TResponse>.Fail($"Lỗi API ({response.StatusCode}): {errContent}");
                }

                var apiResult = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();
                return apiResult ?? ApiResponse<TResponse>.Fail("Phản hồi API rỗng.");
            }
            catch (Exception ex)
            {
                return ApiResponse<TResponse>.Fail($"Lỗi kết nối API: {ex.Message}");
            }
        }

        public static async Task<ApiResponse<AuthResponse>> LoginAsync(string username, string password)
        {
            var request = new LoginRequest { Username = username, Password = password };
            var result = await PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request);
            if (result.Success && result.Data != null)
            {
                SetAuthToken(result.Data.Token);
            }
            return result;
        }
    }
}
