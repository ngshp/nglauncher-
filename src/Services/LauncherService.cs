using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NgpbLauncher.Services
{
    /// <summary>
    /// LauncherService: Menangani komunikasi API dengan Server Backend NGPB.
    /// Bertanggung jawab atas Login, News Feed, dan Session Management.
    /// </summary>
    public class LauncherService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://api.ngpb.com/v1"; // Ganti dengan URL API Anda
        
        // Token sesi yang disimpan sementara di memori
        private string _sessionToken;

        public LauncherService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NGPB-Launcher/1.2.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// Melakukan login ke server dan mendapatkan Session Token.
        /// </summary>
        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                var payload = new { Username = username, Password = password };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BASE_URL}/auth/login", jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        _sessionToken = result.Token;
                        // Simpan token di header untuk request selanjutnya
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionToken);
                    }
                    return result;
                }
                else
                {
                    return new LoginResponse { Success = false, Message = "Server error or invalid credentials." };
                }
            }
            catch (Exception ex)
            {
                return new LoginResponse { Success = false, Message = $"Connection failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Mengambil daftar berita terbaru dari server.
        /// </summary>
        public async Task<NewsItem[]> GetLatestNewsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/news/latest");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<NewsItem[]>();
                }
                return Array.Empty<NewsItem>();
            }
            catch
            {
                return Array.Empty<NewsItem>();
            }
        }

        /// <summary>
        /// Memvalidasi apakah sesi user masih aktif.
        /// </summary>
        public async Task<bool> ValidateSessionAsync()
        {
            if (string.IsNullOrEmpty(_sessionToken)) return false;

            try
            {
                var response = await _httpClient.GetAsync($"{BASE_URL}/auth/validate");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    // --- MODEL DATA API ---

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int Level { get; set; }
        public string Rank { get; set; }
    }

    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Date { get; set; }
    }
}
