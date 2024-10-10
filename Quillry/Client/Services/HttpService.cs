using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Quillry.Client.Helpers;

namespace Quillry.Client.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateProvider _authState;
        public HttpService(HttpClient httpClient, AuthStateProvider authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }

        public async Task<ApiResponse> Get(string url)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{url}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var uri = urlBuiler.ToString();
            var response = await _httpClient.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<ApiResponse> Create(string controller, object obj)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{controller}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            StringContent stringContent = new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var uri = urlBuiler.ToString();
            var response = await _httpClient.PostAsync(uri, stringContent);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<ApiResponse> Update(string controller, object obj)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{controller}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            StringContent stringContent = new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var uri = urlBuiler.ToString();
            var response = await _httpClient.PutAsync(uri, stringContent);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<bool> Delete(string controller, string id)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{controller}/{id}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync(urlBuiler.ToString());
            return response.IsSuccessStatusCode;
        }
    }
}
