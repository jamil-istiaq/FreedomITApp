using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace FreedomITAS.API_Serv
{
    public class DreamscapeService
    {
        private readonly IHttpClientFactory _httpClientFactory;        
        private readonly string _apiKey;
        private readonly string _apiBaseUrl;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public DreamscapeService(IHttpClientFactory httpClientFactory, IOptions<DreamscapeSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = settings.Value.ApiKey;
            _apiBaseUrl = settings.Value.ApiBaseUrl;
        }

        public async Task<HttpResponseMessage> CreateCompanyAsync(object payload)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Generate request ID and signature
            string guid = Guid.NewGuid().ToString();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string requestIdRaw = guid + timestamp;
            string requestId = GenerateMd5(requestIdRaw);
            string signature = GenerateMd5(requestId + _apiKey);

            // 2. Add required headers
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}customers");
            request.Headers.Add("api-request-id", requestId);
            request.Headers.Add("api-signature", signature);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            // 3. Serialize payload
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // 4. Send POST request
            var response = await client.SendAsync(request);
            
            return response;
        }

        private static string GenerateMd5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public async Task<HttpResponseMessage> UpdateCompanyAsync(string customerId, object payload)
        {
            var client = _httpClientFactory.CreateClient();
            var request = BuildSignedRequest(new HttpMethod("PATCH"), $"customers/{customerId}", payload);
            return await client.SendAsync(request);
        }

        private HttpRequestMessage BuildSignedRequest(HttpMethod method, string relativePath, object? body)
        {
            
            string guid = Guid.NewGuid().ToString();
            long tsMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string requestId = GenerateMd5(guid + tsMs);
            string signature = GenerateMd5(requestId + _apiKey);

            var url = $"{_apiBaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";

            var req = new HttpRequestMessage(method, url);
            req.Headers.Add("api-request-id", requestId);
            req.Headers.Add("api-signature", signature);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return req;
        }

        public async Task<HttpResponseMessage> DeleteCompanyAsync(string customerId)
        {
            var client = _httpClientFactory.CreateClient();
            var request = BuildSignedRequest(HttpMethod.Delete, $"customers/{customerId}", null); // reuse your BuildSignedRequest helper
            return await client.SendAsync(request);
        }

    }
}
