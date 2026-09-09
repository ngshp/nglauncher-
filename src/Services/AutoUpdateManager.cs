using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace NgpbLauncher.Services
{
    /// <summary>
    /// AutoUpdateManager: Menangani validasi file lokal vs server dan proses repair/update.
    /// Menggunakan sistem Manifest JSON untuk efisiensi.
    /// </summary>
    public class AutoUpdateManager
    {
        private readonly string _gameDirectory;
        private readonly string _manifestUrl;
        private readonly HttpClient _httpClient;

        public AutoUpdateManager()
        {
            // Asumsi folder game ada di subfolder "Game" relatif terhadap launcher
            _gameDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Game");
            
            // URL manifest update (Ganti dengan URL server NGPB Anda nanti)
            _manifestUrl = "https://api.ngpb.com/v1/launcher/manifest.json";
            
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<UpdateResult> CheckAndApplyUpdatesAsync()
        {
            try
            {
                // 1. Download Manifest Terbaru dari Server
                var manifestJson = await _httpClient.GetStringAsync(_manifestUrl);
                var serverManifest = JsonSerializer.Deserialize<FileManifest>(manifestJson);

                if (serverManifest == null || serverManifest.Files == null)
                {
                    return new UpdateResult { IsSuccess = false, Message = "Invalid server manifest." };
                }

                // 2. Bandingkan dengan File Lokal
                var filesToDownload = new List<string>();
                
                foreach (var remoteFile in serverManifest.Files)
                {
                    string localPath = Path.Combine(_gameDirectory, remoteFile.Path);
                    
                    // Cek apakah file ada dan hash-nya cocok
                    if (!File.Exists(localPath) || !VerifyFileHash(localPath, remoteFile.Hash))
                    {
                        filesToDownload.Add(remoteFile.Path);
                    }
                }

                // 3. Jika tidak ada perubahan, selesai
                if (filesToDownload.Count == 0)
                {
                    return new UpdateResult { IsSuccess = true, Message = "Game is up to date." };
                }

                // 4. Proses Download & Repair
                Directory.CreateDirectory(_gameDirectory);
                
                foreach (var filePath in filesToDownload)
                {
                    string fullLocalPath = Path.Combine(_gameDirectory, filePath);
                    string downloadUrl = $"https://cdn.ngpb.com/game/{filePath}"; // Ganti dengan CDN Anda

                    // Pastikan folder tujuan ada
                    Directory.CreateDirectory(Path.GetDirectoryName(fullLocalPath));

                    // Download file
                    var data = await _httpClient.GetByteArrayAsync(downloadUrl);
                    File.WriteAllBytes(fullLocalPath, data);
                }

                return new UpdateResult 
                { 
                    IsSuccess = true, 
                    Message = $"Repaired {filesToDownload.Count} files.",
                    RestartRequired = false 
                };
            }
            catch (Exception ex)
            {
                return new UpdateResult { IsSuccess = false, Message = $"Update error: {ex.Message}" };
            }
        }

        private bool VerifyFileHash(string filePath, string expectedHash)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                var actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return actualHash == expectedHash.ToLowerInvariant();
            }
            catch
            {
                return false;
            }
        }
    }

    // --- MODEL DATA UNTUK MANIFEST ---
    
    public class FileManifest
    {
        public string Version { get; set; }
        public List<GameFile> Files { get; set; }
    }

    public class GameFile
    {
        public string Path { get; set; }   // Contoh: "Data/Pak01.pak"
        public string Hash { get; set; }   // SHA-256 Hash
        public long Size { get; set; }
    }

    public class UpdateResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool RestartRequired { get; set; }
    }
}
