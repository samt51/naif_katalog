using naif_katalog.Models;

namespace naif_katalog.Services.Abstract
{
    public interface IApiService
    {
        Task<ResponseDto<TResponse>> GetAsync<TResponse>(string url);
        Task<ResponseDto<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest data);
        Task<ResponseDto<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest data);
        Task<ResponseDto<TResponse>> DeleteAsync<TResponse>(string url);
    }
}
