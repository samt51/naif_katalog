using MediatR;
using atasay_katalog.Models;
using atasay_katalog.Services.Abstract;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace atasay_katalog.Core.Features.CategoryFeature.Queries
{
    public class GetAllCategoriesQueryHandler : BaseHandler, IRequestHandler<GetAllCategoriesQueryRequest, ResponseDto<List<CategoryDto>>>
    {
        public GetAllCategoriesQueryHandler(IApiService apiService) : base(apiService)
        {
        }

        public async Task<ResponseDto<List<CategoryDto>>> Handle(GetAllCategoriesQueryRequest request, CancellationToken cancellationToken)
        {
            var apiResult = await _apiService.GetAsync<List<CategoryDto>>("api/Category");
            return apiResult;
        }
    }
}
