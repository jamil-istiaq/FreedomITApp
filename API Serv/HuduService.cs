
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;

namespace FreedomITAS.API_Serv
{
    public class HuduService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HuduSettings _settings;

        public HuduService(IHttpClientFactory httpClientFactory, IOptions<HuduSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }
        
        public async Task<HttpResponseMessage> CreateCompanyAsync(object payload)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-API-KEY", _settings.ApiKey);

            var content = new StringContent(JsonSerializer.Serialize(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await client.PostAsync($"{_settings.ApiBaseUrl}companies", content);
        }
        public async Task<HttpResponseMessage> UpdateCompanyAsync(string companyId, object payload)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-API-KEY", _settings.ApiKey);

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PutAsync($"{_settings.ApiBaseUrl}/companies/{companyId}", content);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(string companyId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("X-API-KEY", _settings.ApiKey);

            var response = await client.DeleteAsync($"{_settings.ApiBaseUrl}/companies/{companyId}");
            return response;
        }

    }
}
