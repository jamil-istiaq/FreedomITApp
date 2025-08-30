using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace FreedomITAS.API_Serv
{
    public class Pax8Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Pax8Settings _settings;

        public Pax8Service(IHttpClientFactory httpClientFactory, IOptions<Pax8Settings> settings)
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
                    { "audience", "https://api.pax8.com" }
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

            return await client.PostAsync($"{_settings.ApiBaseUrl}companies", content);
        }

        public async Task<HttpResponseMessage> UpdateClientAsync(string companyId, object payload)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Use PATCH if partial update is supported
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_settings.ApiBaseUrl}companies/{companyId}")
            {
                Content = content
            };

            return await client.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteClientAsync(string companyId)
        {
            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Confirm with Pax8 docs whether delete is supported; some partners only archive.
            var url = $"{_settings.ApiBaseUrl.TrimEnd('/')}/companies/{companyId}";
            return await client.DeleteAsync(url);
        }


    }
}
