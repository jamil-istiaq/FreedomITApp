using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace FreedomITAS.API_Serv
{
    public class SyncroService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SyncroSettings _settings;

        public SyncroService(IHttpClientFactory httpClientFactory, IOptions<SyncroSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        public async Task<HttpResponseMessage> CreateCompanyAsync(object payload)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Token);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync($"{_settings.ApiBaseUrl}/customers", content);
        }

        public async Task<HttpResponseMessage> UpdateCustomerAsync(string id, object payload)
        {
            var client = _httpClientFactory.CreateClient();
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Put, $"{_settings.ApiBaseUrl}/customers/{id}") { Content = content };
            return await client.SendAsync(req);
        }

        public async Task<HttpResponseMessage> DeleteCustomerOrContactAsync(string id)
        {
            var client = _httpClientFactory.CreateClient();

            // Try contact
            var resp = await client.DeleteAsync($"{_settings.ApiBaseUrl.TrimEnd('/')}/contacts/{id}");
            if (resp.IsSuccessStatusCode || resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                return resp;

            // Fallback to customer
            return await client.DeleteAsync($"{_settings.ApiBaseUrl.TrimEnd('/')}/customers/{id}");
        }

    }
}
