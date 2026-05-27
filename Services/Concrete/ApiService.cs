using naif_katalog.Services.Abstract;
using naif_katalog.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace naif_katalog.Services.Concrete
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpt = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ResponseDto<TResponse>> GetAsync<TResponse>(string url)
            => SendAsync<TResponse>(() => SendWithAuthAsync(HttpMethod.Get, url));

        public Task<ResponseDto<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data)
            => SendAsync<TResponse>(() => SendWithAuthAsync(HttpMethod.Post, url, CreateJsonContent(data)));

        public Task<ResponseDto<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data)
            => SendAsync<TResponse>(() => SendWithAuthAsync(HttpMethod.Put, url, CreateJsonContent(data)));

        public Task<ResponseDto<TResponse>> DeleteAsync<TResponse>(string url)
            => SendAsync<TResponse>(() => SendWithAuthAsync(HttpMethod.Delete, url));

        private async Task<ResponseDto<TResponse>> SendAsync<TResponse>(Func<Task<HttpResponseMessage>> send)
        {
            try
            {
                using var resp = await send();
                return await ReadResponse<TResponse>(resp);
            }
            catch (TaskCanceledException)
            {
                return new ResponseDto<TResponse>
                {
                    isSuccess = false,
                    statusCode = 408,
                    errors = new List<string> { "İstek zaman aşımına uğradı. Lütfen tekrar deneyiniz." }
                };
            }
            catch (HttpRequestException)
            {
                return new ResponseDto<TResponse>
                {
                    isSuccess = false,
                    statusCode = 503,
                    errors = new List<string> { "Servise şu an ulaşılamıyor. Lütfen daha sonra tekrar deneyiniz." }
                };
            }
            catch (Exception)
            {
                return new ResponseDto<TResponse>
                {
                    isSuccess = false,
                    statusCode = 500,
                    errors = new List<string> { "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." }
                };
            }
        }

        private Task<HttpResponseMessage> SendWithAuthAsync(HttpMethod method, string url, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            var token = ResolveToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return _httpClient.SendAsync(request);
        }

        private string? ResolveToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var token = httpContext?.Request.Cookies["jwtToken"];
            var incomingAuthorization = httpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(token)
                && !string.IsNullOrWhiteSpace(incomingAuthorization)
                && incomingAuthorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = incomingAuthorization["Bearer ".Length..].Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                token = httpContext?.User.FindFirst("access_token")?.Value;
            }

            return string.IsNullOrWhiteSpace(token) ? null : token;
        }

        private static StringContent CreateJsonContent<T>(T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOpt);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private static async Task<ResponseDto<T>> ReadResponse<T>(HttpResponseMessage response)
        {
            string json = "";
            try
            {
                json = await response.Content.ReadAsStringAsync();
            }
            catch
            {
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                var errorContent = response.IsSuccessStatusCode ? "" : await response.Content.ReadAsStringAsync();
                return new ResponseDto<T>
                {
                    isSuccess = response.IsSuccessStatusCode,
                    statusCode = (int)response.StatusCode,
                    errors = response.IsSuccessStatusCode ? new List<string>() : new List<string> { string.IsNullOrEmpty(errorContent) ? "Sunucudan geçersiz yanıt alındı." : errorContent }
                };
            }

                        ResponseDto<T> result;
            try {
                if (typeof(T) == typeof(bool) && (json.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) || json.Trim().Equals("false", StringComparison.OrdinalIgnoreCase))) {
                    bool val = bool.Parse(json);
                    result = new ResponseDto<T> { isSuccess = val, data = (T)(object)val, statusCode = (int)response.StatusCode };
                } else {
                    result = JsonSerializer.Deserialize<ResponseDto<T>>(json, JsonOpt);
                }
            } catch {
                result = null;
            }

            if (result == null)
            {
                return new ResponseDto<T>
                {
                    isSuccess = false,
                    statusCode = (int)response.StatusCode,
                    errors = new List<string> { "Sunucudan geçersiz yanıt alındı." }
                };
            }

            result.statusCode = result.statusCode == 0 ? (int)response.StatusCode : result.statusCode;

            if (!response.IsSuccessStatusCode && (result.errors == null || result.errors.Count == 0))
            {
                result.isSuccess = false;
                result.errors = new List<string> { "İşlem sırasında sunucu hatası oluştu." };
            }

            return result;
        }
    }
}




