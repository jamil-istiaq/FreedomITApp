using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace FreedomITAS.API_Serv
{
    public class GoHighLevelService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GoHighLevelSettings _settings;
        private readonly TokenStorageService _tokenStorage;

        public GoHighLevelService(IHttpClientFactory httpClientFactory, IOptions<GoHighLevelSettings> settings, TokenStorageService tokenStorage)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _tokenStorage = tokenStorage;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var (accessToken, refreshToken, expiresAt) = await _tokenStorage.LoadTokensAsync();

                if (DateTime.UtcNow < expiresAt)
                    return accessToken;

                var client = _httpClientFactory.CreateClient();
                var form = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", _settings.ClientId },
                    { "client_secret", _settings.ClientSecret },
                    { "refresh_token", refreshToken }
                };

                var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Failed to refresh token: {response.StatusCode} - {content}");

                using var doc = JsonDocument.Parse(content);
                var newAccessToken = doc.RootElement.GetProperty("access_token").GetString();
                var newRefreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
                var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

                await _tokenStorage.SaveTokensAsync(newAccessToken!, newRefreshToken!, expiresIn);
                return newAccessToken!;
            }
            catch (Exception ex)
            {
                throw new Exception("Token retrieval failed: " + ex.Message);
            }
        }

        public async Task<HttpResponseMessage> CreateContactAsync(object payload)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Version", "2021-07-28");

            var content = new StringContent(JsonSerializer.Serialize(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync($"{_settings.ApiBaseUrl}contacts", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to create contact: {response.StatusCode} - {responseContent}");

            using var doc = JsonDocument.Parse(responseContent);
            return response;
            //return doc.RootElement.GetProperty("id").GetString();
        }

        public async Task ExchangeCodeForTokensAsync(string code)
        {
            var client = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "redirect_uri", "https://app.freedomit.com.au/oauth/callback" },
                { "code", code }
            };

            var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to exchange code: {response.StatusCode} - {content}");

            using var doc = JsonDocument.Parse(content);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();

            await _tokenStorage.SaveTokensAsync(accessToken!, refreshToken!, expiresIn);
        }

        public async Task<HttpResponseMessage> UpdateContactAsync(string contactId, object payload)
        {
            if (string.IsNullOrWhiteSpace(contactId))
                throw new ArgumentException("contactId is required.", nameof(contactId));

            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Remove("Version");
            client.DefaultRequestHeaders.Add("Version", "2021-07-28");

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_settings.ApiBaseUrl.TrimEnd('/')}/contacts/{contactId}";

            var req = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
            return await client.SendAsync(req);
        }

        public async Task<HttpResponseMessage> DeleteContactAsync(string contactId)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Remove("Version");
            client.DefaultRequestHeaders.Add("Version", "2021-07-28");

            var url = $"{_settings.ApiBaseUrl.TrimEnd('/')}/contacts/{contactId}";
            return await client.DeleteAsync(url);
        }



    }
}
