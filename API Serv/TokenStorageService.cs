using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace FreedomITAS.API_Serv
{
    public class TokenStorageService
    {
        private readonly string _tokenFilePath;

        public TokenStorageService(IHostEnvironment env)
        {
            // Running on Azure App Service?
            var home = Environment.GetEnvironmentVariable("HOME"); // set on Azure
            if (!string.IsNullOrEmpty(home))
            {
                _tokenFilePath = Path.Combine(home, "site", "wwwroot", "Data", "TokenStore.json");
            }
            else
            {
                // local/dev fallback
                _tokenFilePath = Path.Combine(env.ContentRootPath, "Data", "TokenStore.json");
            }
        }

        public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> LoadTokensAsync()
        {
            if (!File.Exists(_tokenFilePath))
                throw new FileNotFoundException("Token storage file not found at: " + _tokenFilePath);

            var json = await File.ReadAllTextAsync(_tokenFilePath);
            using var doc = JsonDocument.Parse(json);

            var accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
            var expiresAtStr = doc.RootElement.GetProperty("expires_at").GetString();

            if (!DateTime.TryParse(expiresAtStr, out var expiresAt))
                throw new Exception("expires_at is not a valid ISO-8601 datetime: " + expiresAtStr);

            return (accessToken!, refreshToken!, expiresAt);
        }

        public async Task SaveTokensAsync(string accessToken, string refreshToken, int expiresInSeconds)
        {
            var dir = Path.GetDirectoryName(_tokenFilePath)!;
            Directory.CreateDirectory(dir);

            var data = new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                // refresh 1 minute early
                expires_at = DateTime.UtcNow.AddSeconds(Math.Max(0, expiresInSeconds - 60)).ToString("o")
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_tokenFilePath, json);
        }
    }
}
