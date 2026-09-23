using atasay_katalog.Services.Abstract;

namespace atasay_katalog.Core
{
    public abstract class BaseHandler
    {
        protected readonly IApiService _apiService;

        protected BaseHandler(IApiService apiService)
        {
            _apiService = apiService;
        }
    }
}
