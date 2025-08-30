using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FreedomITAS
{
    public class HaloPSAService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HaloPSA _settings;

        public HaloPSAService(IHttpClientFactory httpClientFactory, IOptions<HaloPSA> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", _settings.ClientId },
                    { "client_secret", _settings.ClientSecret },
                    { "scope", "all" } 
                };

            var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get token: {response.StatusCode} - {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();            
            return token;
        }
        
        public async Task<HttpResponseMessage> CreateClientAsync(object payload)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(JsonSerializer.Serialize(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            return await client.PostAsync($"{_settings.ApiBaseUrl}client", content);            
        }

        public async Task<HttpResponseMessage> UpdateClientAsync(string clientId, object payload)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
           
            var url = $"{_settings.ApiBaseUrl}client/{clientId}";
            var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = content };
            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(string clientId)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Adjust path if your instance uses plural (e.g., /clients/{id})
            var url = $"{_settings.ApiBaseUrl.TrimEnd('/')}/client/{clientId}";
            return await client.DeleteAsync(url);
        }



    }
}
