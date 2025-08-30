using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Azure;
using System.Net;

namespace FreedomITAS.API_Serv
{
    public class ZomentumService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ZomentumSettings _settings;

        public ZomentumService(IHttpClientFactory httpClientFactory, IOptions<ZomentumSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }        

        public async Task<string> RefreshAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                grant_type = "refresh_token",
                refresh_token = _settings.RefreshToken
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.zomentum.com/v1/oauth/access-token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                return jsonDoc.RootElement.GetProperty("access_token").GetString();
            }
            else
            {
                throw new Exception($"Failed to refresh token: {responseContent}");
            }
        }

        private async Task<HttpResponseMessage> PostToZomentumAsync(object payload, string token)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync($"{_settings.ApiBaseUrl.TrimEnd('/')}/client/companies", content);
        }
        public async Task<HttpResponseMessage> CreateClientAsync(object clientPayload)
        {            
            var response = await PostToZomentumAsync(clientPayload, _settings.AccessToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var newAccessToken = await RefreshAccessTokenAsync();
                _settings.AccessToken = newAccessToken; 
                response = await PostToZomentumAsync(clientPayload, newAccessToken);
            }

            //var content = await response.Content.ReadAsStringAsync();
            return response;
                //IsSuccessStatusCode
                //? "Client created successfully in Zomentum."
                //: $"Error creating client: {content}";
           
        }

        private async Task<HttpResponseMessage> PutToZomentumAsync(string companyId, object payload, string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PutAsync($"{_settings.ApiBaseUrl.TrimEnd('/')}/client/companies/{companyId}", content);
        }

        public async Task<HttpResponseMessage> UpdateClientAsync(string companyId, object payload)
        {
            if (string.IsNullOrWhiteSpace(companyId))
                throw new ArgumentException("Zomentum companyId is required.", nameof(companyId));

            var token = _settings.AccessToken;
            var response = await PutToZomentumAsync(companyId, payload, token);

            // Refresh token on 401 and retry once (same as your Create)
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var newAccessToken = await RefreshAccessTokenAsync(); // your existing method
                _settings.AccessToken = newAccessToken;
                response = await PutToZomentumAsync(companyId, payload, newAccessToken);
            }

            return response;
        }

        public async Task<HttpResponseMessage> DeleteClientAsync(string companyId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var url = $"{_settings.ApiBaseUrl.TrimEnd('/')}/client/companies/{companyId}";
            var resp = await client.DeleteAsync(url);

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var newAccessToken = await RefreshAccessTokenAsync();
                _settings.AccessToken = newAccessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
                resp = await client.DeleteAsync(url);
            }

            return resp;
        }




    }
}

